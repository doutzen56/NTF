using System;
using System.Linq.Expressions;

namespace NTF.Provider
{
    /// <summary>
    /// Finds the first sub-expression that is of a specified type
    /// </summary>
    public class TypedSubtreeFinder : ExpressionVisitor
    {
        Expression root;
        Type type;

        private TypedSubtreeFinder(Type type)
        {
            this.type = type;
        }

        public static Expression Find(Expression expression, Type type)
        {
            TypedSubtreeFinder finder = new TypedSubtreeFinder(type);
            finder.Visit(expression);
            return finder.root;
        }

        protected override Expression Visit(Expression exp)
        {
            Expression result = base.Visit(exp);

            // remember the first sub-expression that produces an IQueryable
            if (this.root == null && result != null)
            {
                if (this.type.IsAssignableFrom(result.Type))
                    this.root = result;
            }

            return result;
        }
    }
}