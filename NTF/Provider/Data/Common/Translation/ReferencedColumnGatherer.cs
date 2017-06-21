using System.Collections.Generic;
using System.Linq.Expressions;

namespace NTF.Data.Common
{
    /// <summary>
    /// Gathers all columns referenced by the given expression
    /// </summary>
    public class ReferencedColumnGatherer : DbExpressionVisitor
    {
        HashSet<ColumnExpression> columns = new HashSet<ColumnExpression>();
        bool first = true;

        public static HashSet<ColumnExpression> Gather(Expression expression)
        {
            var visitor = new ReferencedColumnGatherer();
            visitor.Visit(expression);
            return visitor.columns;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            this.columns.Add(column);
            return column;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (first)
            {
                first = false;
                return base.VisitSelect(select);
            }
            return select;
        }
    }
}