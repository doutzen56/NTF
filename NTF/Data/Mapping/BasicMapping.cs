using NTF.Ioc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace NTF.Data.Mapping
{
    /// <summary>
    /// 实体映射基类
    /// </summary>
    public abstract class BasicMapping : ISingleton
    {
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
        protected ConcurrentDictionary<RuntimeTypeHandle, PropertyInfo> AutoIncField = new ConcurrentDictionary<RuntimeTypeHandle, PropertyInfo>();
        /// <summary>
        /// 非自增列
        /// </summary>
        protected ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> NoIncFields = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

        public BasicMapping()
        {
            //nothing to do
        }

        /// <summary>
        /// 获取实体对应的表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract string GetTableName(Type type);
        /// <summary>
        /// 表别名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract string GetTableAlias(Type type);
        /// <summary>
        /// 获取所有字段
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual IEnumerable<PropertyInfo> GetFields(Type type)
        {
            IEnumerable<PropertyInfo> properties;
            if (Fields.TryGetValue(type.TypeHandle, out properties))
            {
                return properties;
            }
            properties = type.GetProperties();
            Fields[type.TypeHandle] = properties;
            return properties;
        }
        /// <summary>
        /// 获取主键
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual IEnumerable<PropertyInfo> GetKeyFields(Type type)
        {
            IEnumerable<PropertyInfo> keyFields;
            if (KeyFields.TryGetValue(type.TypeHandle, out keyFields))
            {
                return keyFields;
            }
            keyFields = this.GetFields(type).Where(a => a.GetCustomAttributes(true).Any(p => p is KeyAttribute));
            KeyFields[type.TypeHandle] = keyFields;
            return keyFields;
        }
        /// <summary>
        /// 获取自增列
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual PropertyInfo GetAutoIncField(Type type)
        {
            PropertyInfo autoIncField;
            if (AutoIncField.TryGetValue(type.TypeHandle, out autoIncField))
            {
                return autoIncField;
            }
            autoIncField = this.GetFields(type).SingleOrDefault(a => a.GetCustomAttributes(true).Any(p => p is DatabaseGeneratedAttribute));
            AutoIncField[type.TypeHandle] = autoIncField;
            return autoIncField;
        }
        /// <summary>
        /// 获取非自增列
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual IEnumerable<PropertyInfo> GetNoIncFields(Type type)
        {
            IEnumerable<PropertyInfo> noIncFields;
            if (NoIncFields.TryGetValue(type.TypeHandle, out noIncFields))
            {
                return noIncFields;
            }
            var autoIncField = this.GetAutoIncField(type);
            noIncFields = this.GetFields(type).Where(a => a != autoIncField);
            NoIncFields[type.TypeHandle] = noIncFields;
            return noIncFields;
        }
    }
}
