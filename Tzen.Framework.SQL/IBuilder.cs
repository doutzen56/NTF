using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tzen.Framework.SQL
{
    public interface IBuilder<T> where T : class
    {
        BuildCommand BuildInsert(T entity);
        BuildCommand BuildInsert(IEnumerable<T> entity);
        BuildCommand BuildGetFirst<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, string orderBy);
        BuildCommand BuildGetList<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, string orderBy);
        BuildCommand BuildUpdate(T entity);
        BuildCommand BuildUpdate(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression);
        BuildCommand BuildUpdateSelect<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression, Expression<Func<T, TResult>> selector, int top);
        BuildCommand BuildDelete(T entity);
        BuildCommand BuildDelete(Expression<Func<T, bool>> predicate);
        BuildCommand BuildPageList<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, string orderBy, int pageIndex, int pageSize);

    }
}
