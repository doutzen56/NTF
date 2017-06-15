using System.Linq.Expressions;

namespace NTF.Provider
{
    /// <summary>
    /// 将<see cref="Expression"/>节点的一个特定实例替换为另一个节点
    /// </summary>
    public class ExpressionReplacer : ExpressionVisitor
    {
        Expression searchFor;
        Expression replaceWith;

        private ExpressionReplacer(Expression searchFor, Expression replaceWith)
        {
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
        }

        public static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
        {
            return new ExpressionReplacer(searchFor, replaceWith).Visit(expression);
        }

        public static Expression ReplaceAll(Expression expression, Expression[] searchFor, Expression[] replaceWith)
        {
            for (int i = 0, n = searchFor.Length; i < n; i++)
            {
                expression = Replace(expression, searchFor[i], replaceWith[i]);
            }
            return expression;
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == this.searchFor)
            {
                return this.replaceWith;
            }
            return base.Visit(exp);
        }
    }
}
