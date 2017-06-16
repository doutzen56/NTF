using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NTF.Provider
{
    /// <summary>
    /// 提供一个生成SQL查询命令的接口，
    /// 扩展自<see cref="IQueryProvider"/>接口的类将实现它，
    /// <see cref="Query{T}"/>中的<see cref="Query{T}.QueryText"/>内部将通过次接口来获取SQL命令
    /// </summary>
    public interface IQueryText
    {
        /// <summary>
        /// 将LINQ表达式转换成SQL命令
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        string GetQueryText(Expression expression);
    }

    /// <summary>
    /// 定义一个数据源，扩展自<see cref="IQueryable{T}"/>。
    /// 所有扩展自<see cref="IQueryProvider"/>的Provider，
    /// 都使用扩展自<see cref="Query{T}"/>的对象来执行查询
    /// </summary>
    public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        IQueryProvider provider;
        Expression expression;

        public Query(IQueryProvider provider)
            : this(provider, null)
        {
        }

        public Query(IQueryProvider provider, Type staticType)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("Provider");
            }
            this.provider = provider;
            this.expression = staticType != null ? Expression.Constant(this, staticType) : Expression.Constant(this);
        }

        public Query(DbProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("Provider");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            this.provider = provider;
            this.expression = expression;
        }
        /// <summary>
        /// 数据源对应的LINQ表达式
        /// </summary>
        public Expression Expression
        {
            get { return this.expression; }
        }
        /// <summary>
        /// 数据源对应的CLR类型
        /// </summary>
        public Type ElementType
        {
            get { return typeof(T); }
        }
        /// <summary>
        /// 与之关联的查询提供程序（Provider）
        /// </summary>
        public IQueryProvider Provider
        {
            get { return this.provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.provider.Execute(this.expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.provider.Execute(this.expression)).GetEnumerator();
        }

        public override string ToString()
        {
            if (this.expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression)this.expression).Value == this)
            {
                return "Query(" + typeof(T) + ")";
            }
            else
            {
                return this.expression.ToString();
            }
        }
        /// <summary>
        /// 数据源对应的SQL命令
        /// </summary>
        public string QueryText
        {
            get 
            {
                IQueryText iqt = this.provider as IQueryText;
                if (iqt != null)
                {
                    return iqt.GetQueryText(this.expression);
                }
                return "";
            }
        }
    }
}
