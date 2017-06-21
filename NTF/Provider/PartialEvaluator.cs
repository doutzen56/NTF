using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NTF.Provider
{
    /// <summary>
    /// 重写表达式树，将局部分离的子表达式树转换成<see cref="ConstantExpression"/>节点，
    /// 如：表达式内容为 a=>new User(){Address="ccc"},则转换成 a=>value(User),
    /// 这块暂不启用
    /// </summary>
    public static class PartialEvaluator
    {
        /// <summary>
        /// 执行独立表达式子节点的计算和替换
        /// </summary>
        /// <param name="expression">表达式根节点</param>
        /// <returns>计算和替换后的表达式</returns>
        public static Expression Eval(Expression expression)
        {
            return Eval(expression, null, null);
        }

        /// <summary>
        /// 执行独立表达式子节点的计算和替换
        /// </summary>
        /// <param name="expression">表达式根节点</param>
        /// <param name="fnCanBeEvaluated">决定给定表达式节点是否可以执行本地计算的函数</param>
        /// <returns>计算和替换后的表达式</returns>
        public static Expression Eval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return Eval(expression, fnCanBeEvaluated, null);
        }

        public static Expression Eval(Expression expression, Func<Expression, bool> fnCanBeEvaluated, Func<ConstantExpression, Expression> fnPostEval)
        {
            if (fnCanBeEvaluated == null)
                fnCanBeEvaluated = PartialEvaluator.CanBeEvaluatedLocally;
            return SubtreeEvaluator.Eval(Nominator.Nominate(fnCanBeEvaluated, expression), fnPostEval, expression);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        /// 表达式树自上而下的计算和替换
        /// </summary>
        class SubtreeEvaluator : ExpressionVisitor
        {
            HashSet<Expression> candidates;
            Func<ConstantExpression, Expression> onEval;

            private SubtreeEvaluator(HashSet<Expression> candidates, Func<ConstantExpression, Expression> onEval)
            {
                this.candidates = candidates;
                this.onEval = onEval;
            }

            internal static Expression Eval(HashSet<Expression> candidates, Func<ConstantExpression, Expression> onEval, Expression exp)
            {
                return new SubtreeEvaluator(candidates, onEval).Visit(exp);
            }

            protected override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                if (this.candidates.Contains(exp))
                {
                    return this.Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private Expression PostEval(ConstantExpression e)
            {
                if (this.onEval != null)
                {
                    return this.onEval(e);
                }
                return e;
            }

            private Expression Evaluate(Expression e)
            {
                return e;
                Type type = e.Type;
                if (e.NodeType == ExpressionType.Convert)
                {
                    // 检查不必要的替换
                    var u = (UnaryExpression)e;
                    if (TypeEx.GetNonNullableType(u.Operand.Type) == TypeEx.GetNonNullableType(type))
                    {
                        e = ((UnaryExpression)e).Operand;
                    }
                }
                if (e.NodeType == ExpressionType.Constant)
                {
                    if (e.Type == type)
                    {
                        return e;
                    }
                    else if (TypeEx.GetNonNullableType(e.Type) == TypeEx.GetNonNullableType(type))
                    {
                        return Expression.Constant(((ConstantExpression)e).Value, type);
                    }
                }
                var me = e as MemberExpression;
                if (me != null)
                {
                    var ce = me.Expression as ConstantExpression;
                    if (ce != null)
                    {
                        return this.PostEval(Expression.Constant(me.Member.GetValue(ce.Value), type));
                    }
                }
                if (type.IsValueType)
                {
                    e = Expression.Convert(e, typeof(object));
                }
                Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(e);
#if NOREFEMIT
                Func<object> fn = ExpressionEvaluator.CreateDelegate(lambda);
#else
                Func<object> fn = lambda.Compile();
#endif
                return this.PostEval(Expression.Constant(fn(), type));
            }
        }

        /// <summary>
        /// 表达式树自下而上的分析，以确定那些节点可以成为计算和替换的子树的一部分
        /// </summary>
        class Nominator : ExpressionVisitor
        {
            Func<Expression, bool> fnCanBeEvaluated;
            HashSet<Expression> candidates;
            bool cannotBeEvaluated;

            private Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                this.candidates = new HashSet<Expression>();
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeEvaluated, Expression expression)
            {
                Nominator nominator = new Nominator(fnCanBeEvaluated);
                nominator.Visit(expression);
                return nominator.candidates;
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {
                return base.VisitConstant(c);
            }

            protected override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!this.cannotBeEvaluated)
                    {
                        if (this.fnCanBeEvaluated(expression))
                        {
                            this.candidates.Add(expression);
                        }
                        else
                        {
                            this.cannotBeEvaluated = true;
                        }
                    }
                    this.cannotBeEvaluated |= saveCannotBeEvaluated;
                }
                return expression;
            }
        }
    }
}