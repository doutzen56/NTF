using System.Linq.Expressions;

namespace NTF.Data.Common
{
    /// <summary>
    /// 将<see cref="Expression"/>节点的一个特定实例替换为另一个节点。
    /// 支持<see cref="DbExpression"/>节点
    /// </summary>
    public class DbExpressionReplacer : DbExpressionVisitor
    {
        Expression searchFor;
        Expression replaceWith;

        private DbExpressionReplacer(Expression searchFor, Expression replaceWith)
        {
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
        }
        /// <summary>
        /// <see cref="Expression"/>替换
        /// </summary>
        /// <param name="expression">表达式源</param>
        /// <param name="searchFor">要替换的表达式</param>
        /// <param name="replaceWith">替换内容</param>
        /// <returns></returns>
        public static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
        {
            return new DbExpressionReplacer(searchFor, replaceWith).Visit(expression);
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
