using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tzen.Framwork.Utility;

namespace Tzen.Framwork.Expressions
{
    internal class PartialEvaluator
    {
        public static Expression Eval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        public static Expression Eval(Expression expression)
        {
            return Eval(expression, CanBeEvaluatedLocally);
        }

        public static Expression Eval<T>(Expression expression)
        {
            return Eval(expression, CanBeEvaluatedLocally<T>);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        private static bool CanBeEvaluatedLocally<T>(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter && (expression.Type != typeof(T) || (expression is MemberExpression));
        }

        internal class SubtreeEvaluator : ExpressionVisitor
        {
            HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }

            internal Expression Eval(Expression exp)
            {
                return Visit(exp);
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                if (candidates.Contains(exp))
                {
                    return Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private Expression Evaluate(Expression e)
            {
                Type type = e.Type;
                if (e.NodeType == ExpressionType.Convert)
                {
                    var u = (UnaryExpression)e;
                    if (TypeHelper.GetNonNullableType(u.Operand.Type) == TypeHelper.GetNonNullableType(type))
                    {
                        e = ((UnaryExpression)e).Operand;
                    }
                    else if (u.Operand.Type.IsEnum && u.Operand.NodeType == ExpressionType.MemberAccess)
                    {
                        var value = Convert.ChangeType(MemberAccessor.Process(u.Operand as MemberExpression), e.Type);
                        return Expression.Constant(value, e.Type);
                    }
                }

                switch (e.NodeType)
                {
                    case ExpressionType.Constant:
                        if (e.Type == type)
                            return e;
                        else if (TypeHelper.GetNonNullableType(e.Type) == TypeHelper.GetNonNullableType(type))
                            return Expression.Constant(((ConstantExpression)e).Value, type);
                        break;

                    case ExpressionType.MemberAccess:
                        //TODO: 性能问题，暂不开放条件语句中方法直接调用
                        var value = MemberAccessor.Process(e as MemberExpression);
                        return Expression.Constant(value, e.Type);

                    default:
                        throw new InvalidOperationException(string.Format("条件表达式中不支持 {0} 类型表达式", e.NodeType));
                }

                throw new InvalidOperationException(string.Format("条件表达式未知表达式类型 {0}", e.NodeType));

            }
        }

        internal class Nominator : ExpressionVisitor
        {
            Func<Expression, bool> fnCanBeEvaluated;
            HashSet<Expression> candidates;
            bool cannotBeEvaluated;

            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                candidates = new HashSet<Expression>();
                Visit(expression);
                return candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = cannotBeEvaluated;
                    cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!cannotBeEvaluated)
                    {
                        if (fnCanBeEvaluated(expression))
                        {
                            candidates.Add(expression);
                        }
                        else
                        {
                            cannotBeEvaluated = true;
                        }
                    }
                    cannotBeEvaluated |= saveCannotBeEvaluated;
                }
                return expression;
            }
        }
    }
}
