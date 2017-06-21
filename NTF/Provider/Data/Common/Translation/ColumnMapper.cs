using System.Collections.Generic;
using System.Linq.Expressions;

namespace NTF.Data.Common
{
    /// <summary>
    /// 重写所有列引用的别名（用一个新生成的列别名来重写他们）
    /// </summary>
    public class ColumnMapper : DbExpressionVisitor
    {
        HashSet<TableAlias> oldAliases;
        TableAlias newAlias;

        private ColumnMapper(IEnumerable<TableAlias> oldAliases, TableAlias newAlias)
        {
            this.oldAliases = new HashSet<TableAlias>(oldAliases);
            this.newAlias = newAlias;
        }

        public static Expression Map(Expression expression, TableAlias newAlias, IEnumerable<TableAlias> oldAliases)
        {
            return new ColumnMapper(oldAliases, newAlias).Visit(expression);
        }

        public static Expression Map(Expression expression, TableAlias newAlias, params TableAlias[] oldAliases)
        {
            return Map(expression, newAlias, (IEnumerable<TableAlias>)oldAliases);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (this.oldAliases.Contains(column.Alias))
            {
                return new ColumnExpression(column.Type, column.QueryType, this.newAlias, column.Name);
            }
            return column;
        }
    }
}
