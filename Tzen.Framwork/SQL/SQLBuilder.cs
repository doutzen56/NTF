using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Tzen.Framwork.Expressions;

namespace Tzen.Framwork.SQL
{
    public class SQLBuilder<T> where T : class
    {
        #region 属性

        private static string _table = $"{typeof(T).Name}";

        public static string SelectFieldString;
        public static string NoKeyFieldString;
        public static string KeyFieldString;

        public static PropertyInfo[] Fields;
        public static PropertyInfo[] KeyFields;
        public static PropertyInfo[] NoIncFields;

        private static string InsertSQL;
        private static string InsertNotExistsSQL;
        private static string UpdateSQL;
        private static string DeleteSQL;

        public static PropertyInfo AutoIncrement;

        #endregion

        #region 构造函数
        static SQLBuilder()
        {
            InitFields();
            InitFieldString();
            InitInsertSQL();
            InitInsertNotExistsSQL();
            InitUpdateSQL();
            InitDeleteSQL();
        }
        #endregion
        
        #region 初始化字段属性

        private static void InitFields()
        {
            Fields = typeof(T).GetProperties();
            KeyFields = Fields.Where(f => Attribute.IsDefined(f, typeof(KeyAttribute))).ToArray();
            AutoIncrement = Fields.Where(f => Attribute.IsDefined(f, typeof(DatabaseGeneratedAttribute))).SingleOrDefault();
            if (AutoIncrement != null)
                NoIncFields = Fields.Where(f => f.Name != AutoIncrement.Name).ToArray();
            else
                NoIncFields = Fields;
        }

        #endregion

        #region Init FieldString

        private static void InitFieldString()
        {
            var builder = new StringBuilder();

            var wroted = false;
            foreach (var field in Fields)
            {
                if (wroted) builder.Append(",");
                builder.Append(field.Name);
                wroted = true;
            }
            SelectFieldString = builder.ToString();

            builder.Clear();
            wroted = false;
            foreach (var field in NoIncFields)
            {
                if (wroted) builder.Append(",");
                builder.Append(field.Name);
                wroted = true;
            }
            NoKeyFieldString = builder.ToString();

            builder.Clear();
            wroted = false;
            foreach (var key in KeyFields)
            {
                if (wroted) builder.Append(" AND");
                builder.Append(key.Name);
                builder.Append("=");
                builder.Append("@");
                builder.Append(key.Name);
                wroted = true;
            }

            KeyFieldString = builder.ToString();
        }

        #endregion

        #region Init SQL

        private static void InitDeleteSQL()
        {
            var builder = new StringBuilder();
            builder.Append("DELETE FROM ");
            builder.Append(_table);
            builder.Append(" WHERE ");
            var wroteSet = false;
            foreach (var key in KeyFields)
            {
                if (wroteSet)
                {
                    builder.Append(" AND ");
                }
                builder.Append(key.Name);
                builder.Append("=");
                builder.Append("@");
                builder.Append(key.Name);
                wroteSet = true;
            }
            DeleteSQL = builder.ToString();
        }

        private static void InitUpdateSQL()
        {
            var builder = new StringBuilder();
            builder.Append("UPDATE ");
            builder.Append(_table);
            builder.Append(" SET ");
            bool wroteSet = false;
            var normalFields = NoIncFields.Except(KeyFields);
            foreach (var field in normalFields)
            {
                if (wroteSet)
                {
                    builder.Append(",");
                }
                builder.Append(field.Name);
                builder.Append("=");
                builder.Append("@");
                builder.Append(field.Name);
                wroteSet = true;
            }
            builder.Append(" WHERE ");
            wroteSet = false;
            foreach (var key in KeyFields)
            {
                if (wroteSet)
                {
                    builder.Append(" AND ");
                }
                builder.Append(key.Name);
                builder.Append("=");
                builder.Append("@");
                builder.Append(key.Name);
                wroteSet = true;
            }
            UpdateSQL = builder.ToString();
        }

        private static void InitInsertSQL()
        {
            var builder = new StringBuilder();
            builder.Append("INSERT INTO ");
            builder.Append(_table);
            builder.Append(" (");
            bool wroteSet = false;
            var fields = NoIncFields;
            foreach (var field in fields)
            {
                if (wroteSet)
                {
                    builder.Append(",");
                }
                builder.Append(field.Name);
                wroteSet = true;
            }
            builder.Append(") VALUES (");
            wroteSet = false;
            foreach (var field in NoIncFields)
            {
                if (wroteSet)
                {
                    builder.Append(",");
                }
                builder.Append("@");
                builder.Append(field.Name);
                wroteSet = true;
            }
            builder.Append(")");
            if (AutoIncrement != null)
            {
                builder.Append(";SELECT SCOPE_IDENTITY();");
            }
            InsertSQL = builder.ToString();
        }

        private static void InitInsertNotExistsSQL()
        {
            var builder = new StringBuilder();
            builder.Append("INSERT INTO ");
            builder.Append(_table);
            builder.Append(" (");
            bool wroteSet = false;
            var fields = NoIncFields;
            foreach (var field in fields)
            {
                if (wroteSet)
                {
                    builder.Append(",");
                }
                builder.Append(field.Name);
                wroteSet = true;
            }
            builder.Append(") SELECT ");
            wroteSet = false;
            foreach (var field in NoIncFields)
            {
                if (wroteSet)
                {
                    builder.Append(",");
                }
                builder.Append("@");
                builder.Append(field.Name);
                wroteSet = true;
            }
            builder.Append(" ");
            InsertNotExistsSQL = builder.ToString();
        }

        #endregion

        #region 数据库命令

        private static void addIf(StringBuilder builder, SQLCmd cmd)
        {
            if (!string.IsNullOrEmpty(cmd.SQL))
            {
                builder.Append(" WHERE ");
                builder.Append(cmd.SQL);
                builder.Append(" ");
            }
        }

        public static string GetCountSQL(string dbLock)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT COUNT(1) FROM ");
            builder.Append(_table);
            builder.Append(dbLock);
            return builder.ToString();
        }

        public static string GetSelectSQL(int top, string orderby, string dbLock)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT ");
            if (top > 0)
                builder.AppendFormat("TOP {0} ", top);
            builder.Append("*");
            builder.Append(" FROM ");
            builder.Append(_table);
            builder.Append(dbLock);
            builder.Append(" ");
            builder.Append(orderby);
            return builder.ToString();
        }

        public static SQLCmd BuildCountCommand(Expression<Func<T, bool>> predicate, string dbLock)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT COUNT(1) FROM ");
            builder.Append(_table);
            builder.Append(dbLock);
            var cmd = new PredicateReader().Translate(predicate);
            addIf(builder, cmd);
            cmd.SQL = builder.ToString();
            return cmd;
        }

        public static SQLCmd BuildGetCommand<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, string orderby, string dbLock)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT TOP 1 ");

            if (selector == null)
                builder.Append("*");
            else
                ReadSelector(builder, selector);

            builder.Append(" FROM ");
            builder.Append(_table);
            builder.Append(dbLock);
            var cmd = new PredicateReader().Translate(predicate);
            addIf(builder, cmd);
            if (!orderby.IsNullOrWhiteSpace())
            {
                builder.Append(" ORDER BY ");
                builder.Append(orderby);
            }
            cmd.SQL = builder.ToString();
            return cmd;
        }

        public static SQLCmd BuildListCommand<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, string orderby, int? top, string dbLock)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT ");

            if (top > 0)
                builder.AppendFormat("TOP {0} ", top);

            if (selector.IsNull())
                builder.Append("*");
            else
                ReadSelector(builder, selector);

            builder.Append(" FROM ");
            builder.Append(_table);
            builder.Append(dbLock);
            var cmd = new PredicateReader().Translate(predicate);
            addIf(builder, cmd);
            if (!orderby.IsNullOrWhiteSpace())
            {
                builder.Append(" ORDER BY ");
                builder.Append(orderby);
            }
            cmd.SQL = builder.ToString();
            return cmd;
        }

        public static SQLCmd BuildPageCommand<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector,
            string orderby,
            int pageIndex,
            int pageSize,
            string dbLock
            )
        {
            var builder = new StringBuilder();

            var cmd = new PredicateReader().Translate(predicate);
            builder.Append("SELECT COUNT(1) FROM ");
            builder.Append(_table);
            builder.Append(dbLock);
            addIf(builder, cmd);
            builder.Append(";");

            builder.Append("SELECT ");

            if (selector == null)
                builder.Append("*");
            else
                ReadSelector(builder, selector);
            builder.Append(" FROM(");

            builder.Append("SELECT ");
            if (selector == null)
                builder.Append("*");
            else
                ReadSelector(builder, selector);

            builder.AppendFormat(",ROW_NUMBER() OVER(ORDER BY {0}) AS RowNumber ", orderby);
            builder.AppendFormat("FROM {0}{1} A", _table, dbLock);
            addIf(builder, cmd);
            builder.Append(" ) B");
            builder.AppendFormat(" WHERE RowNumber > {0} AND RowNumber <= {1}", (pageIndex - 1) * pageSize, pageIndex * pageSize);
            cmd.SQL = builder.ToString();

            return cmd;
        }

        public static SQLCmd BuildAddCommand(T entity)
        {
            var parameters = new DynamicParameters();
            foreach (var field in NoIncFields)
            {
                var value = field.GetValue(entity);
                parameters.Add(field.Name, value);
            }
            return new SQLCmd { SQL = InsertSQL, Parameters = parameters };
        }

        public static SQLCmd BuildAddCommand(IEnumerable<T> entities)
        {
            var parameters = new DynamicParameters();
            var pindex = 0;

            var builder = new StringBuilder();
            builder.Append("INSERT INTO ");
            builder.Append(_table);
            builder.Append(" (");
            bool wroteSet = false;
            foreach (var field in NoIncFields)
            {
                if (wroteSet)
                    builder.Append(",");

                builder.Append(field.Name);
                wroteSet = true;
            }
            builder.Append(") VALUES ");

            bool itemWroteSet = false;
            foreach (var item in entities)
            {
                if (itemWroteSet)
                    builder.Append(",");

                builder.Append("(");
                wroteSet = false;
                foreach (var field in NoIncFields)
                {
                    var pname = "p" + pindex++;
                    if (wroteSet)
                    {
                        builder.Append(",");
                    }
                    builder.Append("@");
                    builder.Append(pname);
                    var value = field.GetValue(item);
                    parameters.Add(pname, value);
                    wroteSet = true;
                }
                builder.Append(")");
                itemWroteSet = true;
            }

            return new SQLCmd { SQL = builder.ToString(), Parameters = parameters };
        }

        public static SQLCmd BuildAddCommand(T entity, Expression<Func<T, bool>> predicate)
        {
            var builder = new StringBuilder();
            builder.Append(InsertNotExistsSQL);
            builder.Append("WHERE NOT EXISTS(SELECT TOP 1 * FROM ");
            builder.Append(_table);
            var cmd = new PredicateReader().Translate(predicate);
            addIf(builder, cmd);

            builder.Append(")");
            if (AutoIncrement != null)
                builder.Append(";IF(@@ROWCOUNT > 0) BEGIN	SELECT SCOPE_IDENTITY(); END ELSE BEGIN SELECT NULL; END;");
            else
                builder.Append("; SELECT @@ROWCOUNT;");

            cmd.SQL = builder.ToString();
            foreach (var field in NoIncFields)
            {
                var value = field.GetValue(entity);
                cmd.Parameters.Add(field.Name, value);
            }
            return cmd;
        }

        public static SQLCmd BuildUpdateCommand(T entity)
        {
            var parameters = new DynamicParameters();
            foreach (var field in Fields)
            {
                //TODO: 委托代替反射
                var value = field.GetValue(entity);
                parameters.Add(field.Name, value);
            }
            return new SQLCmd { SQL = UpdateSQL, Parameters = parameters };
        }

        public static SQLCmd BuildUpdateCommand(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression)
        {
            var memberInitExpression = updateExpression.Body as MemberInitExpression;
            memberInitExpression.ThrowIf(
                m => m == null,
                "The updateExpression must be of type MemberInitExpression."
                );

            var cmd = new PredicateReader().Translate(predicate);
            var builder = new StringBuilder();
            memberInitExpression = PartialEvaluator.Eval<T>(memberInitExpression) as MemberInitExpression;
            builder.Append("UPDATE ");
            builder.Append(_table);
            builder.Append(" SET ");
            bool wroteSet = false;
            foreach (var binding in memberInitExpression.Bindings)
            {
                if (wroteSet)
                    builder.Append(",");
                var constant = ((binding as MemberAssignment).Expression as ConstantExpression);
                builder.Append(binding.Member.Name);
                builder.Append(" = @");
                builder.Append(binding.Member.Name);
                cmd.Parameters.Add("@" + binding.Member.Name, constant.Value);
                wroteSet = true;
            }
            addIf(builder, cmd);
            cmd.SQL = builder.ToString();
            return cmd;
        }

        public static SQLCmd BuildUpdateSelectCommand<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, T>> updateExpression,
            Expression<Func<T, TResult>> selector,
            int top)
        {
            var memberInitExpression = updateExpression.Body as MemberInitExpression;
            memberInitExpression.ThrowIf(
                m => m == null,
                "The updateExpression must be of type MemberInitExpression."
                );

            var cmd = new PredicateReader().Translate(predicate);
            var builder = new StringBuilder();
            memberInitExpression = PartialEvaluator.Eval<T>(memberInitExpression) as MemberInitExpression;
            builder.Append("UPDATE TOP(");
            builder.Append(top);
            builder.Append(") ");
            builder.Append(_table);
            builder.Append(" WITH (UPDLOCK, READPAST) SET ");
            bool wroteSet = false;
            foreach (var binding in memberInitExpression.Bindings)
            {
                if (wroteSet)
                    builder.Append(",");
                var constant = ((binding as MemberAssignment).Expression as ConstantExpression);
                builder.Append(binding.Member.Name);
                builder.Append(" = @");
                builder.Append(binding.Member.Name);
                cmd.Parameters.Add("@" + binding.Member.Name, constant.Value);
                wroteSet = true;
            }
            builder.Append(" OUTPUT ");

            if (selector.IsNull())
                builder.Append("INSERTED.*");
            else
                ReadSelector(builder, selector, true);

            addIf(builder, cmd);
            cmd.SQL = builder.ToString();
            return cmd;
        }

        public static SQLCmd BuildDeleteCommand(T entity)
        {
            var parameters = new DynamicParameters();
            foreach (var field in KeyFields)
            {
                var value = field.GetValue(entity);
                parameters.Add(field.Name, value);
            }
            return new SQLCmd { SQL = DeleteSQL, Parameters = parameters };
        }

        public static SQLCmd BuildDeleteCommand(Expression<Func<T, bool>> predicate)
        {
            var builder = new StringBuilder();
            builder.Append("DELETE FROM ");
            builder.Append(_table);
            var cmd = new PredicateReader().Translate(predicate);
            addIf(builder, cmd);
            cmd.SQL = builder.ToString();
            return cmd;
        }

        #endregion

        #region 辅助方法

        private static void ReadSelector<TResult>(StringBuilder builder, Expression<Func<T, TResult>> selector, bool inserted = false)
        {
            var memberInitExpression = selector.Body as MemberInitExpression;
            if (memberInitExpression != null)
            {
                var bindings = memberInitExpression.Bindings;
                bool wroteSet = false;
                foreach (var binding in bindings)
                {
                    var assginMengent = ((binding as MemberAssignment).Expression as MemberExpression);
                    if (wroteSet)
                        builder.Append(",");
                    if (inserted)
                        builder.Append("INSERTED.");
                    builder.Append(assginMengent.Member.Name);
                    builder.Append(" AS ");
                    builder.Append(binding.Member.Name);
                    wroteSet = true;
                }
                return;
            }

            // 单个字段
            var singleField = selector.Body as MemberExpression;
            if (singleField != null)
            {
                if (inserted)
                    builder.Append("INSERTED.");
                builder.Append(singleField.Member.Name);
                return;
            }

            // 匿名类
            builder.Append(inserted ? DynamicEntity<TResult>.InsertedSqlFields : DynamicEntity<TResult>.SqlFields);
        }

        #endregion
    }
}
