using NTF.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NTF.Data.Common
{
    /// <summary>
    /// 定义查询提供程序（Provider）的语言规则
    /// </summary>
    public abstract class QueryLanguage
    {
        public abstract QueryTypeSystem TypeSystem { get; }
        public abstract Expression GetGeneratedIdExpression(MemberInfo member);

        public virtual string Quote(string name)
        {
            return name;
        }

        public virtual bool AllowsMultipleCommands
        {
            get { return false; }
        }

        public virtual bool AllowSubqueryInSelectWithoutFrom
        {
            get { return false; }
        }

        public virtual bool AllowDistinctInAggregates
        {
            get { return false; }
        }

        public virtual Expression GetRowsAffectedExpression(Expression command)
        {
            return new FunctionExpression(typeof(int), "@@ROWCOUNT", null);
        }

        public virtual bool IsRowsAffectedExpressions(Expression expression)
        {
            FunctionExpression fex = expression as FunctionExpression;
            return fex != null && fex.Name == "@@ROWCOUNT";
        }

        public virtual Expression GetOuterJoinTest(SelectExpression select)
        {
            var aliases = DeclaredAliasGatherer.Gather(select.From);
            var joinColumns = JoinColumnGatherer.Gather(aliases, select).ToList();
            if (joinColumns.Count > 0)
            {
                foreach (var jc in joinColumns)
                {
                    foreach (var col in select.Columns)
                    {
                        if (jc.Equals(col.Expression))
                        {
                            return jc;
                        }
                    }
                }
                return joinColumns[0];
            }
            return Expression.Constant(1, typeof(int?));
        }

        public virtual ProjectionExpression AddOuterJoinTest(ProjectionExpression proj)
        {
            var test = this.GetOuterJoinTest(proj.Select);
            var select = proj.Select;
            ColumnExpression testCol = null;
            foreach (var col in select.Columns)
            {
                if (test.Equals(col.Expression))
                {
                    var colType = this.TypeSystem.GetColumnType(test.Type);
                    testCol = new ColumnExpression(test.Type, colType, select.Alias, col.Name);
                    break;
                }
            }
            if (testCol == null)
            {
                testCol = test as ColumnExpression;
                string colName = (testCol != null) ? testCol.Name : "Test";
                colName = proj.Select.Columns.GetAvailableColumnName(colName);
                var colType = this.TypeSystem.GetColumnType(test.Type);
                select = select.AddColumn(new ColumnDeclaration(colName, test, colType));
                testCol = new ColumnExpression(test.Type, colType, select.Alias, colName);
            }
            var newProjector = new OuterJoinedExpression(testCol, proj.Projector);
            return new ProjectionExpression(select, newProjector, proj.Aggregator);
        }

        class JoinColumnGatherer
        {
            HashSet<TableAlias> aliases;
            HashSet<ColumnExpression> columns = new HashSet<ColumnExpression>();

            private JoinColumnGatherer(HashSet<TableAlias> aliases)
            {
                this.aliases = aliases;
            }

            public static HashSet<ColumnExpression> Gather(HashSet<TableAlias> aliases, SelectExpression select)
            {
                var gatherer = new JoinColumnGatherer(aliases);
                gatherer.Gather(select.Where);
                return gatherer.columns;
            }

            private void Gather(Expression expression)
            {
                BinaryExpression b = expression as BinaryExpression;
                if (b != null)
                {
                    switch (b.NodeType)
                    {
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                            if (IsExternalColumn(b.Left) && GetColumn(b.Right) != null)
                            {
                                this.columns.Add(GetColumn(b.Right));
                            }
                            else if (IsExternalColumn(b.Right) && GetColumn(b.Left) != null)
                            {
                                this.columns.Add(GetColumn(b.Left));
                            }
                            break;
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            if (b.Type == typeof(bool) || b.Type == typeof(bool?))
                            {
                                this.Gather(b.Left);
                                this.Gather(b.Right);
                            }
                            break;
                    }
                }
            }

            private ColumnExpression GetColumn(Expression exp)
            {
                while (exp.NodeType == ExpressionType.Convert)
                    exp = ((UnaryExpression)exp).Operand;
                return exp as ColumnExpression;
            }

            private bool IsExternalColumn(Expression exp)
            {
                var col = GetColumn(exp);
                if (col != null && !this.aliases.Contains(col.Alias))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// 确定CLR类型是否与查询语言中的数据类型相对应
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool IsScalar(Type type)
        {
            type = TypeEx.GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return false;
                case TypeCode.Object:
                    return
                        type == typeof(DateTimeOffset) ||
                        type == typeof(TimeSpan) ||
                        type == typeof(Guid) ||
                        type == typeof(byte[]);
                default:
                    return true;
            }
        }

        public virtual bool IsAggregate(MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
            {
                if (method.DeclaringType == typeof(Queryable)
                    || method.DeclaringType == typeof(Enumerable))
                {
                    switch (method.Name)
                    {
                        case "Count":
                        case "LongCount":
                        case "Sum":
                        case "Min":
                        case "Max":
                        case "Average":
                            return true;
                    }
                }
            }
            var property = member as PropertyInfo;
            if (property != null
                && property.Name == "Count"
                && typeof(IEnumerable).IsAssignableFrom(property.DeclaringType))
            {
                return true;
            }
            return false;
        }

        public virtual bool AggregateArgumentIsPredicate(string aggregateName)
        {
            return aggregateName == "Count" || aggregateName == "LongCount";
        }

        /// <summary>
        /// 确定给定的表达式可以表示为<see cref="SelectExpression"/>的一个列
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual bool CanBeColumn(Expression expression)
        {
            return this.MustBeColumn(expression);
        }

        public virtual bool MustBeColumn(Expression expression)
        {
            switch (expression.NodeType)
            {
                case (ExpressionType)DbExpressionType.Column:
                case (ExpressionType)DbExpressionType.Scalar:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.AggregateSubquery:
                case (ExpressionType)DbExpressionType.Aggregate:
                    return true;
                default:
                    return false;
            }
        }

        public virtual QueryLinguist CreateLinguist(QueryTranslator translator)
        {
            return new QueryLinguist(this, translator);
        }
    }
    /// <summary>
    /// 一个拥有特定SQL语言定义，以及将LINQ表达式翻译成特定SQL语言功能的基类，
    /// 用于做SQL翻译工作
    /// </summary>
    public class QueryLinguist
    {
        QueryLanguage language;
        QueryTranslator translator;

        public QueryLinguist(QueryLanguage language, QueryTranslator translator)
        {
            this.language = language;
            this.translator = translator;
        }

        public QueryLanguage Language 
        {
            get { return this.language; }
        }

        public QueryTranslator Translator
        {
            get { return this.translator; }
        }

        /// <summary>
        /// 将LINQ表达式翻译成特定的SQL语言
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            expression = UnusedColumnRemover.Remove(expression);
            expression = RedundantColumnRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);
            var rewritten = CrossApplyRewriter.Rewrite(this.language, expression);
            rewritten = CrossJoinRewriter.Rewrite(rewritten);
            if (rewritten != expression)
            {
                expression = rewritten;
                expression = UnusedColumnRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
                expression = RedundantColumnRemover.Remove(expression);
            }

            return expression;
        }

        /// <summary>
        /// 将查询表达式转换为该查询语言的SQL命令
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual string Format(Expression expression)
        {
            return CmdFormatter.Format(expression);
        }

        /// <summary>
        /// 确定哪些子表达式必须是Parameter类型
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Parameterize(Expression expression)
        {
            return Parameterizer.Parameterize(this.language, expression);
        }
    }
}