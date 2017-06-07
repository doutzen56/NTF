using System;
using System.Linq.Expressions;

namespace NTF
{
    public class OrderBy<T> : OrderBy where T : class
    {
        /// <summary>
        /// OrderBy与string之间的隐式转换
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(OrderBy<T> value)
        {
            return value?.orderBy.TrimStart(',');
        }
        /// <summary>
        /// 升序
        /// </summary>
        /// <typeparam name="SortField"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual OrderBy<T> Asc<SortField>(Expression<Func<T, SortField>> expression)
        {
            this.orderBy += "ORDER BY ";
            this.orderBy += (expression.Body as MemberExpression).Member.Name + ASC;
            return this;
        }
        /// <summary>
        /// 降序
        /// </summary>
        /// <typeparam name="SortField"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual OrderBy<T> Desc<SortField>(Expression<Func<T, SortField>> expression)
        {
            this.orderBy += "ORDER BY ";
            this.orderBy += (expression.Body as MemberExpression).Member.Name + DESC;
            return this;
        }

    }
    public class OrderBy
    {
        protected string orderBy;
        protected const string ASC = " ASC";
        protected const string DESC = " DESC";
        protected OrderBy(string str = "")
        {
            this.orderBy = str;
        }
        /// <summary>
        /// OrderBy与string之间的隐式转换
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(OrderBy value)
        {
            return value?.orderBy.TrimStart(',');
        }
        /// <summary>
        /// 降序排序
        /// </summary>
        /// <param name="getDescField">指定排序字段，如：Desc(()=>{ return "Age,Name";})</param>
        /// <returns>getDescField指定的排序字段并按照降序排序，如："Age,Name DESC"</returns>
        public virtual string Desc(Func<string> getDescField)
        {
            this.orderBy += "ORDER BY ";
            this.orderBy += getDescField();
            this.orderBy += DESC;
            return this;
        }
        /// <summary>
        /// 升序排序
        /// </summary>
        /// <param name="getAscField">指定排序字段，如：Asc(()=> { return "Age,Name";})</param>
        /// <returns>getAscField指定的排序字段并按照升序排序，如："Age,Name ASC"</returns>
        public virtual string Asc(Func<string> getAscField)
        {
            this.orderBy += "ORDER BY ";
            this.orderBy += getAscField();
            this.orderBy += ASC;
            return this;
        }
    }
}
