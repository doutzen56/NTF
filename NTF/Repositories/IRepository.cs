using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NTF.SQL
{
    public interface IRepository<T> where T : class
    {
        #region 01 查询 Get
        IQueryable<T> GetAll();
        TResult Query<TResult>(Func<IQueryable<T>, TResult> queryMethod);
        T Get(Expression<Func<T, bool>> predicate);
        TResult Get<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
        List<T> GetList(Expression<Func<T, bool>> predicate);
        List<T> GetList(Expression<Func<T, bool>> predicate, Func<OrderBy<T>, OrderBy<T>> orderby = null, int top = 0);
        List<TResult> GetList<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
        List<TResult> GetList<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, Func<OrderBy<T>, OrderBy<T>> orderby = null, int top = 0);
        #endregion

        #region 02 添加 Add/Insert
        int Add(T entity);
        void Add(IEnumerable<T> entities);

        void BatchInsert(IEnumerable<T> entities);
        #endregion

        #region 03 更新 Update/UpdateSelect
        int Update(T entity);
        void Update(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression);
        List<T> UpdateSelect(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression);
        List<T> UpdateSelect(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression, int top);
        List<TResult> UpdateSelect<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression, Expression<Func<T, TResult>> selector);
        List<TResult> UpdateSelect<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression, Expression<Func<T, TResult>> selector, int top);
        #endregion

        #region 04 删除 Delete
        int Delete(T entity);
        void Delete(Expression<Func<T, bool>> predicate);
        void Delete(IEnumerable<T> entities);
        #endregion

        #region 05 分页 PageList
        PageList<T> PageList(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize);
        PageList<T> PageList(Expression<Func<T, bool>> predicate, Func<OrderBy<T>, OrderBy<T>> orderby, int pageIndex, int pageSize);
        PageList<TResult> PageList<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, int pageIndex, int pageSize);
        PageList<TResult> PageList<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, Func<OrderBy<T>, OrderBy<T>> orderby, int pageIndex, int pageSize);
        #endregion
    }
}
