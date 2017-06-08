using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace NTF.Provider.Data.Common
{
    public class NamedValueGatherer : DbExpressionVisitor
    {
        HashSet<NamedValueExpression> namedValues = new HashSet<NamedValueExpression>(new NamedValueComparer());

        private NamedValueGatherer()
        {
        }

        public static ReadOnlyCollection<NamedValueExpression> Gather(Expression expr)
        {
            NamedValueGatherer gatherer = new NamedValueGatherer();
            gatherer.Visit(expr);
            return gatherer.namedValues.ToList().AsReadOnly();
        }

        protected override Expression VisitNamedValue(NamedValueExpression value)
        {
            this.namedValues.Add(value);
            return value;
        }

        class NamedValueComparer : IEqualityComparer<NamedValueExpression>
        {
            public bool Equals(NamedValueExpression x, NamedValueExpression y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(NamedValueExpression obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}