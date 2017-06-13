using System;
using System.Linq;
using System.Linq.Expressions;

namespace NTF.Provider
{
    /// <summary>
    /// 定义获取数据实体上下文Provider
    /// </summary>
    public interface IDbContextProvider : IQueryProvider
    {
        IDbContext<T> GetDbContext<T>(string tableName);
        IDbContext GetDbContext(Type type, string tableName);
        bool CanBeEvaluatedLocally(Expression expression);
        bool CanBeParameter(Expression expression);
    }
    /// <summary>
    /// 数据库上下文操作定义
    /// </summary>
    public interface IDbContext : IQueryable, INonQuery
    {
        new IDbContextProvider Provider { get; }
        string TableName { get; }
        object GetById(object id);
        int Insert(object instance);
        int Update(object instance);
        int Delete(object instance);
        int InsertOrUpdate(object instance);
    }
    /// <summary>
    /// 数据库上下文操作定义
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDbContext<T> : IQueryable<T>, IDbContext, INonQuery<T>
    {
        new T GetById(object id);
        int Insert(T instance);
        int Update(T instance);
        int Delete(T instance);
        int InsertOrUpdate(T instance);
    }
}