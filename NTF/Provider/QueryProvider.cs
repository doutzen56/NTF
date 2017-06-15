using NTF.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NTF.Provider
{
    /// <summary>
    /// 定义一个扩展自<see cref="IQueryProvider"/>用于执行LINQ查询的Provider
    /// </summary>
    public abstract class QueryProvider : IQueryProvider, IQueryText
    {
        protected QueryProvider()
        {
        }
        /// <summary>
        /// 构造一个 <see cref="IQueryable{T}"/> 对象，该对象可计算指定表达式所表示的查询
        /// </summary>
        /// <typeparam name="TElement"> 返回的 <see cref="IQueryable{T}"/> 的元素的类型</typeparam>
        /// <param name="expression">表示 LINQ 查询的表达式</param>
        /// <returns>一个 <see cref="IQueryable{T}"/>，它可计算指定表达式所表示的查询</returns>
        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            return new Query<TElement>(this, expression);
        }

        /// <summary>
        /// 构造一个 <see cref="IQueryable"/> 对象，该对象可计算指定表达式所表示的查询
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式</param>
        /// <returns>一个 <see cref="IQueryable"/>，它可计算指定表达式所表示的查询</returns>
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = TypeEx.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// 执行指定表达式所表示的强类型查询
        /// </summary>
        /// <typeparam name="TResult">执行查询所生成的值的类型</typeparam>
        /// <param name="expression">表示 LINQ 查询的表达式</param>
        /// <returns>执行指定查询所生成的值</returns>
        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            return (TResult)this.Execute(expression);
        }
        /// <summary>
        /// 解析表达式所等价的SQL语句
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式</param>
        /// <returns></returns>
        public abstract string GetQueryText(Expression expression);
        /// <summary>
        /// 执行指定表达式所表示的查询
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式</param>
        /// <returns>执行指定查询所生成的值</returns>
        public abstract object Execute(Expression expression);
    }
}
