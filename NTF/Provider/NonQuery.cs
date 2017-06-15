using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NTF.Provider
{
    /// <summary>
    /// 非查询操作（增,删,改）
    /// </summary>
    public interface INonQuery : IQueryable
    {
    }
    /// <summary>
    /// 非查询操作（增,删,改）
    /// </summary>
    public interface INonQuery<T> : INonQuery, IQueryable<T>
    {
    }
    /// <summary>
    /// <see cref="INonQuery{T}"/>扩展，扩展增删改方法
    /// </summary>
    public static class NonQuery
    {
        /// <summary>
        /// 添加一个新实体并返回结果
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="TResult">返回实体类型</typeparam>
        /// <param name="collection"></param>
        /// <param name="instance">要添加的实例实体</param>
        /// <param name="resultSelector">返回结果</param>
        /// <returns></returns>
        public static TResult Insert<T, TResult>(this INonQuery<T> collection, T instance, Expression<Func<T, TResult>> resultSelector)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T), typeof(TResult)),
                collection.Expression,
                Expression.Constant(instance),
                resultSelector != null ? (Expression)Expression.Quote(resultSelector) : Expression.Constant(null, typeof(Expression<Func<T, TResult>>))
                );
            return (TResult)collection.Provider.Execute(callMyself);
        }
        /// <summary>
        /// 添加新实体
        /// </summary>
        /// <typeparam name="T">要添加的实例实体</typeparam>
        /// <param name="collection"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static int Insert<T>(this INonQuery<T> collection, T instance)
        {
            return Insert<T, int>(collection, instance, null);
        }
        /// <summary>
        /// 批量更新实体并返回更新结果
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="TResult">返回结果中的实体类型</typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate">更新条件</param>
        /// <param name="updateExpression">更新内容</param>
        /// <param name="resultSelector">返回结果</param>
        /// <returns></returns>
        public static TResult Update<T, TResult>(this INonQuery<T> collection, Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression, Expression<Func<T, TResult>> resultSelector)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T), typeof(TResult)),
                collection.Expression,
                predicate != null ? (Expression)Expression.Quote(predicate) : Expression.Constant(null, typeof(Expression<Func<T, bool>>)),
                updateExpression,
                resultSelector != null ? (Expression)Expression.Quote(resultSelector) : Expression.Constant(null, typeof(Expression<Func<T, TResult>>))
                );
            return (TResult)collection.Provider.Execute(callMyself);
        }
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate">更新条件</param>
        /// <param name="updateExpression">更新内容</param>
        /// <returns></returns>
        public static int Update<T>(this INonQuery<T> collection, Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                collection.Expression,
                predicate != null ? (Expression)Expression.Quote(predicate) : Expression.Constant(null, typeof(Expression<Func<T, bool>>)),
                updateExpression
                );
            return (int)collection.Provider.Execute(callMyself);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="collection"></param>
        /// <param name="instance">要更新的实例实体</param>
        /// <returns></returns>
        public static int Update<T>(this INonQuery<T> collection, T instance)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                collection.Expression,
                Expression.Constant(instance)
                );
            return (int)collection.Provider.Execute(callMyself);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="instance">要删除的实例实体</param>
        /// <returns></returns>
        public static int Delete<T>(this INonQuery<T> collection, T instance)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                collection.Expression,
                Expression.Constant(instance)
                );
            return (int)collection.Provider.Execute(callMyself);
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate">删除条件</param>
        /// <returns></returns>
        public static int Delete<T>(this INonQuery<T> collection, Expression<Func<T, bool>> predicate)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                collection.Expression,
                predicate != null ? (Expression)Expression.Quote(predicate) : Expression.Constant(null, typeof(Expression<Func<T, bool>>))
                );
            return (int)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// 批量增、删、改操作
        /// </summary>
        /// <typeparam name="TDbContext">数据集上下文类型</typeparam>
        /// <typeparam name="TModel">实体类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="collection">数据集上下文</param>
        /// <param name="instances">批量操作实例</param>
        /// <param name="fnOperation">批量操作，增、删、改</param>
        /// <param name="batchSize">批量操作数量</param>
        /// <param name="isLazy">是否延时加载</param>
        /// <returns>返回批量操作对应的结果/结果集</returns>
        public static IEnumerable<TResult> Batch<TDbContext, TModel, TResult>(this INonQuery<TDbContext> collection, IEnumerable<TModel> instances, Expression<Func<INonQuery<TDbContext>, TModel, TResult>> fnOperation, int batchSize, bool isLazy)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(TDbContext), typeof(TModel), typeof(TResult)),
                collection.Expression,
                Expression.Constant(instances),
                fnOperation != null ? (Expression)Expression.Quote(fnOperation) : Expression.Constant(null, typeof(Expression<Func<INonQuery<TDbContext>, TModel, TResult>>)),
                Expression.Constant(batchSize),
                Expression.Constant(isLazy)
                );
            return (IEnumerable<TResult>)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// 批量增、删、改操作
        /// </summary>
        /// <typeparam name="TDbContext">数据集上下文类型</typeparam>
        /// <typeparam name="TModel">实体类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="collection">数据集上下文</param>
        /// <param name="instances">批量操作实例</param>
        /// <param name="fnOperation">批量操作，增、删、改</param>
        /// <returns>返回批量操作对应的结果/结果集</returns>
        public static IEnumerable<TResult> Batch<TDbContext, TModel, TResult>(this INonQuery<TDbContext> collection, IEnumerable<TModel> instances, Expression<Func<INonQuery<TDbContext>, TModel, TResult>> fnOperation)
        {
            return Batch<TDbContext, TModel, TResult>(collection, instances, fnOperation, 50, false);
        }
    }
}