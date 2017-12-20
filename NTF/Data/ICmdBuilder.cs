using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NTF.Data
{
    /// <summary>
    /// 构建T-SQL语句
    /// </summary>
    public interface ICmdBuilder
    {
        SQLCommand BuildAddCommand<TEntity>(TEntity model) where TEntity : class;
        SQLCommand BuildAddCommand<TEntity>(TEntity model, Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        SQLCommand BuildBatchAddCommand<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        SQLCommand BuildUpdateCommand<TEntity>(TEntity model) where TEntity : class;
        SQLCommand BuildBatchUpdateCommand<TEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateExpression) 
            where TEntity : class;
        SQLCommand BuildDeleteCommand<TEntity>(TEntity model) where TEntity : class;
        SQLCommand BuildDeleteCommand<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        SQLCommand BuildGetCommand<TEntity, TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector,string orderby) 
            where TEntity : class;
        SQLCommand BuildListCommand<TEntity, TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, string orderby, int? top)
            where TEntity : class;
        SQLCommand BuildPageCommand<TEntity, TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector,string orderby,int pageIndex,int pageSize) 
            where TEntity : class;
    }
}
