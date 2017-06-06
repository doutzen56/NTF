using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tzen.Framework.Provider
{

    public static class Evaluator {
        /// <summary>
        /// 表达式执行前子节点赋值与替换
        /// </summary>
        /// <param name="expression">表达式树的根</param>
        /// <param name="fnCanBeEvaluated">标识给定的表达式节点是否可以作为局部函数的一部分</param>
        /// <returns>一个新的子节点</returns>
        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated) {
            return SubtreeEvaluator.Eval(Nominator.Nominate(fnCanBeEvaluated, expression), expression);
        }

        /// <summary>
        ///表达式执行前子节点赋值与替换
        /// </summary>
        /// <param name="expression">表达式树的根</param>
        /// <returns>一个新的子节点</returns>
        public static Expression PartialEval(Expression expression) {
            return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression) {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        ///子节点赋值替换，自上而下
        /// </summary>
        class SubtreeEvaluator: DbExpressionVisitor {
            HashSet<Expression> candidates;

            private SubtreeEvaluator(HashSet<Expression> candidates) {
                this.candidates = candidates;
            }

            internal static Expression Eval(HashSet<Expression> candidates, Expression exp) {
                return new SubtreeEvaluator(candidates).Visit(exp);
            }

            protected override Expression Visit(Expression exp) {
                if (exp == null) {
                    return null;
                }
                if (this.candidates.Contains(exp)) {
                    return this.Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private Expression Evaluate(Expression e) {
                if (e.NodeType == ExpressionType.Constant) {
                    return e;
                }
                Type type = e.Type;
                if (type.IsValueType) {
                    e = Expression.Convert(e, typeof(object));
                }
                Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(e);
                Func<object> fn = lambda.Compile();
                return Expression.Constant(fn(), type);
            }
        }

        /// <summary>
        /// 执行自下而上的分析，以确定哪些节点可能作为评估子树的一部分
        /// </summary>
        class Nominator : DbExpressionVisitor {
            Func<Expression, bool> fnCanBeEvaluated;
            HashSet<Expression> candidates;
            bool cannotBeEvaluated;

            private Nominator(Func<Expression, bool> fnCanBeEvaluated) {
                this.candidates = new HashSet<Expression>();
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeEvaluated, Expression expression) {
                Nominator nominator = new Nominator(fnCanBeEvaluated);
                nominator.Visit(expression);
                return nominator.candidates;
            }

            protected override Expression Visit(Expression expression) {
                if (expression != null) {
                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!this.cannotBeEvaluated) {
                        if (this.fnCanBeEvaluated(expression)) {
                            this.candidates.Add(expression);
                        }
                        else {
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