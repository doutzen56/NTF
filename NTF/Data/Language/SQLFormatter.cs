using NTF.Data.Mapping;
using NTF.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NTF.Data.Language
{
    /// <summary>
    /// 构建T-SQL语句
    /// </summary>
    public abstract class SQLFormatter : ICmdBuilder
    {
        StringBuilder cmdText;
        BasicMapping mapping;
        protected static Dictionary<RuntimeTypeHandle, string> insertSQL;
        protected static Dictionary<RuntimeTypeHandle, string> updateSQL;
        protected static Dictionary<RuntimeTypeHandle, string> deleteSQL;
        static SQLFormatter() 
        {
            insertSQL = new Dictionary<RuntimeTypeHandle, string>();
            updateSQL = new Dictionary<RuntimeTypeHandle, string>();
            deleteSQL = new Dictionary<RuntimeTypeHandle, string>();
        }
        public SQLFormatter()
        {
            cmdText = new StringBuilder();
            mapping = IocManager.Instance.Resolve<BasicMapping>();
        }
        /// <summary>
        /// T-SQL语句中列名或者表名引用处理，如Type,User等字段处理为[Type],[User]
        /// </summary>
        /// <param name="name">目标字段/表名</param>
        /// <returns></returns>
        protected virtual string Quote(string name)
        {
            return name;
        }

        protected abstract string ScopeIdentity();
        /// <summary>
        /// 追加T-SQL命令到CommandText
        /// </summary>
        /// <param name="value"></param>
        protected void Append(object value)
        {
            this.cmdText.Append(value);
        }
        /// <summary>
        /// 输出T-SQL语句
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.cmdText.ToString();
        }

        #region T-SQL 语句初始化
        /// <summary>
        /// 初始化Insert操作SQL语句
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual string InitInsertSQL(Type type)
        {
            if (insertSQL[type.TypeHandle].IsNullOrEmpty())
            {
                var builder = new StringBuilder();
                builder.Append("INSERT INTO ");
                builder.Append(mapping.GetTableName(type));
                builder.Append(" (");
                bool isFirst = true;
                var noIncFields = mapping.GetNoIncFields(type);
                foreach (var item in noIncFields)
                {
                    if (!isFirst)
                    {
                        builder.Append(",");
                    }
                    builder.Append(this.Quote(item.Name));
                    isFirst = false;
                }
                builder.Append(") VALUES (");
                isFirst = true;
                foreach (var item in noIncFields)
                {
                    if (!isFirst)
                    {
                        builder.Append(",");
                    }
                    builder.Append("@");
                    builder.Append(item.Name);
                    isFirst = false;
                }
                builder.Append(");");
                if (!mapping.GetAutoIncField(type).IsNull())
                {
                    builder.Append(this.ScopeIdentity());
                }
                insertSQL[type.TypeHandle] = builder.ToString();
            }
            return insertSQL[type.TypeHandle];
        }

        protected virtual string InitUpdateSQL(Type type)
        {

            return updateSQL[type.TypeHandle];
        }

        protected virtual string InitDeleteSQL(Type type)
        {
            return updateSQL[type.TypeHandle];
        }
        #endregion

        #region ICmdBuilder接口实现
        public virtual SQLCommand BuildAddCommand<TEntity>(TEntity model) where TEntity : class
        {
            var noIncFields = mapping.GetNoIncFields(typeof(TEntity));
            return null;
        }

        public virtual SQLCommand BuildAddCommand<TEntity>(TEntity model, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var type = predicate.Type;
            throw new NotImplementedException();
        }

        public virtual SQLCommand BuildBatchAddCommand<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public virtual SQLCommand BuildBatchUpdateCommand<TEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateExpression) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public virtual SQLCommand BuildDeleteCommand<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public virtual SQLCommand BuildDeleteCommand<TEntity>(TEntity model) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public virtual SQLCommand BuildGetCommand<TEntity, TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, string orderby) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public virtual SQLCommand BuildListCommand<TEntity, TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, string orderby, int? top) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public virtual SQLCommand BuildPageCommand<TEntity, TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, string orderby, int pageIndex, int pageSize) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public virtual SQLCommand BuildUpdateCommand<TEntity>(TEntity model) where TEntity : class
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 私有函数
        #endregion

    }
}
