using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Tzen.Framework.SQL
{
    public abstract class CmdBuilder<T> : IBuilder<T> where T : class
    {
        #region 属性成员
        /// <summary>
        /// 表名（实体名称）
        /// </summary>
        protected static string TableName { get; private set; }
        protected CmdTemplate CmdTemplate;
        /// <summary>
        /// 主键列
        /// </summary>
        protected static ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> PrimaryKeyFields
        {
            get;
            private set;
        }
        /// <summary>
        /// 自动生成列
        /// </summary>
        protected static ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> AutomaticFields
        {
            get;
            private set;
        }
        /// <summary>
        /// 所有属性列
        /// </summary>
        protected static ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties
        {
            get;
            private set;
        }
        /// <summary>
        /// 查询语句缓存
        /// </summary>
        protected static ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries
        {
            get;
            private set;
        }
        #endregion

        #region 构造函数
        static CmdBuilder()
        {
            Init();
        }

        public CmdBuilder()
            : this(new DefaultCmdTemplate())
        {
        }
        public CmdBuilder(CmdTemplate cmdTemplate)
        {
            this.CmdTemplate = cmdTemplate;
        }
        #endregion

        #region 公有静态方法，不需要重写
        public static Dictionary<ReplaceArgs, string> GetInsertTmpArgs()
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            dic.Add(ReplaceArgs.TableName, TableName);
            dic.Add(ReplaceArgs.Columns, GetInsertColumns());
            dic.Add(ReplaceArgs.Values, GetInsertValues());
            return dic;
        }
        public static Dictionary<ReplaceArgs, string> GetUpdateTmpArgs()
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            dic.Add(ReplaceArgs.TableName, TableName);
            dic.Add(ReplaceArgs.NameValues, GetUpdateNameValues());
            return dic;
        }
        public static BuildCommand GetWhereByKeys()
        {
            var columns = GetPrimaryKey(typeof(T));
            var context = new BuildCommand();
            context.SQL = string.Join(" AND ", columns.Select(a => a.Name + " = @" + a.Name));
            return context;
        }
        public static BuildCommand GetWhere(Expression<Func<T, bool>> predicate)
        {
            return new QueryVisitor().Translate(predicate);
        }
        public static string GetInsertColumns()
        {
            var flag = false;
            var builder = new StringBuilder();
            var automatic = GetAutomatic(typeof(T));
            var columns = GetTypeProperties(typeof(T)).Except(automatic);
            foreach (var item in columns)
            {
                if (flag)
                    builder.Append(",");
                builder.Append("{0}".Fmt(item.Name));
                flag = true;
            }
            return builder.ToString();
        }
        public static string GetInsertValues()
        {
            var flag = false;
            var builder = new StringBuilder();
            var automatic = GetAutomatic(typeof(T));
            var columns = GetTypeProperties(typeof(T)).Except(automatic);
            foreach (var item in columns)
            {
                if (flag)
                    builder.Append(",");
                builder.Append("@{0}".Fmt(item.Name));
                flag = true;
            }
            return builder.ToString();
        }
        public static string GetUpdateNameValues()
        {
            var builder = new StringBuilder();
            var automatic = GetAutomatic(typeof(T));
            var primaryKey = GetPrimaryKey(typeof(T));
            var columns = GetTypeProperties(typeof(T)).Except(automatic).Except(primaryKey);
            return string.Join(",", columns.Select(a => a.Name + " = @" + a.Name));
        }
        public static string GetSelectColumns<TResult>(Expression<Func<T, TResult>> selector)
        {
            if (selector.IsNull())
                return " * ";
            var flag = false;
            var builder = new StringBuilder();
            var properties = GetTypeProperties(typeof(TResult));
            foreach (var item in properties)
            {
                if (flag)
                    builder.Append(",");
                builder.Append("{0}".Fmt(item.Name));
                flag = true;
            }
            return builder.ToString();
        }
        public static IEnumerable<PropertyInfo> GetTypeProperties(Type type)
        {
            IEnumerable<PropertyInfo> typeProperyies;
            if (TypeProperties.TryGetValue(type.TypeHandle, out typeProperyies))
            {
                return typeProperyies;
            }
            typeProperyies = typeof(T).GetProperties();
            TypeProperties[type.TypeHandle] = typeProperyies;
            return typeProperyies;
        }
        public static IEnumerable<PropertyInfo> GetPrimaryKey(Type type)
        {
            IEnumerable<PropertyInfo> primaryKey;
            if (PrimaryKeyFields.TryGetValue(type.TypeHandle, out primaryKey))
            {
                return primaryKey;
            }
            primaryKey = GetTypeProperties(type).Where(a => Attribute.IsDefined(a, typeof(KeyAttribute))).ToList();
            PrimaryKeyFields[type.TypeHandle] = primaryKey;
            return primaryKey;
        }
        public static IEnumerable<PropertyInfo> GetAutomatic(Type type)
        {
            IEnumerable<PropertyInfo> automatic;
            if (PrimaryKeyFields.TryGetValue(type.TypeHandle, out automatic))
            {
                return automatic;
            }
            automatic = GetTypeProperties(type).Where(a => Attribute.IsDefined(a, typeof(DatabaseGeneratedAttribute))).ToList();
            AutomaticFields[type.TypeHandle] = automatic;
            return automatic;
        }
        #endregion

        #region 私有方法
        private static void Init()
        {
            var type = typeof(T);
            TableName = type.Name;
            PrimaryKeyFields = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
            AutomaticFields = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
            TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
            GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        }
        #endregion

        #region IBuilder接口实现
        public virtual BuildCommand BuildDelete(Expression<Func<T, bool>> predicate)
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            var context = GetWhere(predicate);
            dic.Add(ReplaceArgs.Where, context.SQL);
            dic.Add(ReplaceArgs.TableName, TableName);
            context.SQL = CmdTemplate.Replace(CmdTemplate.DeleteTmp, dic);
            return context;
        }
        public virtual BuildCommand BuildDelete(T entity)
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            var context = GetWhereByKeys();
            dic.Add(ReplaceArgs.Where, context.SQL);
            dic.Add(ReplaceArgs.TableName, TableName);
            context.SQL = CmdTemplate.Replace(CmdTemplate.DeleteTmp, dic);
            foreach (var item in GetPrimaryKey(typeof(T)))
            {
                var value = item.GetValue(entity);
                context.Parameters.Add(item.Name, value);
            }
            return context;
        }
        public virtual BuildCommand BuildGetFirst<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, string orderBy)
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            var context = GetWhere(predicate);
            dic.Add(ReplaceArgs.Columns, GetSelectColumns(selector));
            dic.Add(ReplaceArgs.TableName, TableName);
            dic.Add(ReplaceArgs.Where, context.SQL);
            dic.Add(ReplaceArgs.OrderBy, orderBy);
            context.SQL = CmdTemplate.Replace(CmdTemplate.GetFirstTmp, dic);
            return context;
        }
        public virtual BuildCommand BuildGetList<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, string orderBy)
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            var context = GetWhere(predicate);
            dic.Add(ReplaceArgs.Columns, GetSelectColumns(selector));
            dic.Add(ReplaceArgs.TableName, TableName);
            dic.Add(ReplaceArgs.Where, context.SQL);
            dic.Add(ReplaceArgs.OrderBy, orderBy);
            context.SQL = CmdTemplate.Replace(CmdTemplate.GetListTmp, dic);
            return context;
        }
        public virtual BuildCommand BuildInsert(IEnumerable<T> entity)
        {
            return null;
        }
        public virtual BuildCommand BuildInsert(T entity)
        {
            var dic = GetInsertTmpArgs();
            var context = new BuildCommand();
            context.SQL = CmdTemplate.Replace(CmdTemplate.InsertTmp, dic);
            foreach (var item in GetTypeProperties(typeof(T)).Except(GetAutomatic(typeof(T))))
            {
                var value = item.GetValue(entity);
                context.Parameters.Add(item.Name, value);
            }
            return context;
        }
        public virtual BuildCommand BuildPageList<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, string orderBy, int pageIndex, int pageSize)
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            var context = GetWhere(predicate);
            dic.Add(ReplaceArgs.Columns, GetSelectColumns(selector));
            dic.Add(ReplaceArgs.TableName, TableName);
            dic.Add(ReplaceArgs.Where, context.SQL);
            dic.Add(ReplaceArgs.OrderBy, orderBy);
            dic.Add(ReplaceArgs.LeftThan, ((pageIndex - 1) * pageSize).ToString());
            dic.Add(ReplaceArgs.RightThan, (pageIndex * pageSize).ToString());
            context.SQL = CmdTemplate.Replace(CmdTemplate.GetPageListTmp, dic);
            return context;
        }
        public virtual BuildCommand BuildUpdate(T entity)
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            var context = GetWhereByKeys();
            dic.Add(ReplaceArgs.TableName, TableName);
            dic.Add(ReplaceArgs.Where, context.SQL);
            dic.Add(ReplaceArgs.NameValues, GetUpdateNameValues());
            context.SQL = CmdTemplate.Replace(CmdTemplate.UpdateTmp, dic);
            foreach (var item in GetTypeProperties(typeof(T)).Except(GetAutomatic(typeof(T))))
            {
                var value = item.GetValue(entity);
                context.Parameters.Add(item.Name, value);
            }
            return context;
        }
        public virtual BuildCommand BuildUpdate(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression)
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            var context = GetWhere(predicate);
            //dic.Add(ReplaceArgs.Columns, GetSelectColumns(selector));
            dic.Add(ReplaceArgs.Where, context.SQL);
            dic.Add(ReplaceArgs.TableName, TableName);
            dic.Add(ReplaceArgs.NameValues, GetUpdateNameValues());
            context.SQL = CmdTemplate.Replace(CmdTemplate.UpdateTmp, dic);
            return context;
        }
        public virtual BuildCommand BuildUpdateSelect<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression, Expression<Func<T, TResult>> selector, int top)
        {
            var dic = new Dictionary<ReplaceArgs, string>();
            var context = GetWhere(predicate);
            dic.Add(ReplaceArgs.Where, context.SQL);
            dic.Add(ReplaceArgs.TableName, TableName);
            context.SQL = CmdTemplate.Replace(CmdTemplate.DeleteTmp, dic);
            return context;
        }
        #endregion
    }
}
