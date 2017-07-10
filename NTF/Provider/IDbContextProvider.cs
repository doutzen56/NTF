using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NTF.Provider
{
    /// <summary>
    /// 获取数据源的Provider
    /// </summary>
    public interface IDbContextProvider : IQueryProvider
    {
        /// <summary>
        /// 获取指定强类型的数据源
        /// </summary>
        /// <typeparam name="T">数据表对应的实体类型</typeparam>
        /// <param name="tableName">数据表名称</param>
        /// <returns>返回一个扩展自<see cref="IQueryable{T}"/>的强类型可查询数据源</returns>
        IDbContext<T> GetDbContext<T>(string tableName);
        /// <summary>
        /// 获取指定类型的数据源
        /// </summary>
        /// <param name="type">指定的实体类型</param>
        /// <param name="tableName">对应的数据表名称</param>
        /// <returns>返回一个扩展自<see cref="IQueryable"/>的可查询数据源</returns>
        IDbContext GetDbContext(Type type, string tableName);
        /// <summary>
        /// 判断<see cref="Expression"/>是否可以执行本地计算
        /// </summary>
        /// <param name="expression">待判断表达式</param>
        /// <returns></returns>
        bool CanBeEvaluatedLocally(Expression expression);
        /// <summary>
        /// 判断<see cref="Expression"/>是否可参数化
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        bool CanBeParameter(Expression expression);
    }
    /// <summary>
    /// 数据源操作定义
    /// </summary>
    public interface IDbContext : IQueryable, INonQuery
    {
        /// <summary>
        /// 获取构建数据源对应的Provider
        /// </summary>
        new IDbContextProvider Provider { get; }
        /// <summary>
        /// 数据源对应的表名
        /// </summary>
        string TableName { get; }
        /// <summary>
        /// 根据实体Id获取实体对象
        /// </summary>
        /// <param name="id">实体Id</param>
        /// <returns></returns>
        object GetById(object id);
        /// <summary>
        /// 向数据源中插入一条数据
        /// </summary>
        /// <param name="instance">要插入的实体实例</param>
        /// <returns></returns>
        int Insert(object instance);
        /// <summary>
        /// 更新数据源中对应的实例
        /// </summary>
        /// <param name="instance">要更新实体实例</param>
        /// <returns></returns>
        int Update(object instance);
        /// <summary>
        /// 删除数据源中对应的实例
        /// </summary>
        /// <param name="instance">要删除的实体实例</param>
        /// <returns></returns>
        int Delete(object instance);
    }
    /// <summary>
    /// 数据源操作定义
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDbContext<T> : IQueryable<T>,IEnumerable<T>, IDbContext, INonQuery<T>
    {
        /// <summary>
        /// 根据实体Id获取实体对象
        /// </summary>
        /// <param name="id">实体Id</param>
        /// <returns></returns>
        new T GetById(object id);
        /// <summary>
        /// 向数据源中插入一条数据
        /// </summary>
        /// <param name="instance">要插入的实体实例</param>
        /// <returns></returns>
        int Insert(T instance);
        /// <summary>
        /// 删除数据源中对应的实例
        /// </summary>
        /// <param name="instance">要删除的实体实例</param>
        /// <returns></returns>
        int Update(T instance);
        /// <summary>
        /// 更新数据源中对应的实例
        /// </summary>
        /// <param name="instance">要更新实体实例</param>
        /// <returns></returns>
        int Delete(T instance);
    }
}