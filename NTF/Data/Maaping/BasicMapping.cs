using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NTF.Data.Maaping
{
    /// <summary>
    /// 实体映射基类
    /// </summary>
    public abstract class BasicMapping
    {
        /// <summary>
        /// 实体对应的表名
        /// </summary>
        public virtual string TableName { get; }
        /// <summary>
        /// 实体对应的表别名
        /// </summary>
        public virtual string TableAlias { get; }
        /// <summary>
        /// 实体属性
        /// </summary>
        public static PropertyInfo[] Properties;
        public static PropertyInfo[] KeyProperties;
        public static PropertyInfo AutoIncrement;

        /// <summary>
        /// 获取实体对应的表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual string GetTableName(Type type)
        {
            return type.Name;
        }
    }
}
