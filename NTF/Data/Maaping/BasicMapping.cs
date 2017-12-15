using System;
using System.Collections.Concurrent;
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
        /// 所有字段
        /// </summary>
        protected ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> Fields = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        /// <summary>
        /// 主键
        /// </summary>
        protected ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyFields = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        /// <summary>
        /// 自增列
        /// </summary>
        protected PropertyInfo AutoIncFiekd = null;
        /// <summary>
        /// 非自增列
        /// </summary>
        protected ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> NoIncFields = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

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
