using System;
using System.Linq.Expressions;

namespace NTF.Provider
{
    /// <summary>
    /// 查找指定类型的第一个子表达式
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
            if (this.root == null && result != null)
            {
                if (this.type.IsAssignableFrom(result.Type))
                    this.root = result;
            }
            return result;
        }
    }
}