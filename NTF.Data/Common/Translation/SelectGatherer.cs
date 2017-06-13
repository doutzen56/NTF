using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace NTF.Data.Common
{
    /// <summary>
    /// returns the list of SelectExpressions accessible from the source expression
    /// </summary>
    public class SelectGatherer : DbExpressionVisitor
    {
        List<SelectExpression> selects = new List<SelectExpression>();

        public static ReadOnlyCollection<SelectExpression> Gather(Expression expression)
        {
            var gatherer = new SelectGatherer();
            gatherer.Visit(expression);
            return gatherer.selects.AsReadOnly();
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            this.selects.Add(select);
            return select; // don't visit sub-queries
        }
    }
}