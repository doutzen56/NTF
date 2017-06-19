using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace NTF.Data.Common
{
    /// <summary>
    /// 调用<see cref="ColumnProjector.ProjectColumns"/>结果
    /// </summary>
    public sealed class ProjectedColumns
    {
        Expression projector;
        ReadOnlyCollection<ColumnDeclaration> columns;

        public ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            this.projector = projector;
            this.columns = columns;
        }

        public Expression Projector
        {
            get { return this.projector; }
        }

        public ReadOnlyCollection<ColumnDeclaration> Columns
        {
            get { return this.columns; }
        }
    }

    /// <summary>
    /// 将表达式拆分为两部分，
    /// 1.必须在
    /// </summary>
    public class ColumnProjector : DbExpressionVisitor
    {
        QueryLanguage language;
        Dictionary<ColumnExpression, ColumnExpression> map;
        List<ColumnDeclaration> columns;
        HashSet<string> columnNames;
        HashSet<Expression> candidates;
        HashSet<TableAlias> existingAliases;
        TableAlias newAlias;
        int iColumn;

        private ColumnProjector(QueryLanguage language, Expression expression, IEnumerable<ColumnDeclaration> existingColumns, TableAlias newAlias, IEnumerable<TableAlias> existingAliases)
        {
            this.language = language;
            this.newAlias = newAlias;
            this.existingAliases = new HashSet<TableAlias>(existingAliases);
            this.map = new Dictionary<ColumnExpression, ColumnExpression>();
            if (existingColumns != null)
            {
                this.columns = new List<ColumnDeclaration>(existingColumns);
                this.columnNames = new HashSet<string>(existingColumns.Select(c => c.Name));
            }
            else
            {
                this.columns = new List<ColumnDeclaration>();
                this.columnNames = new HashSet<string>();
            }
            this.candidates = Nominator.Nominate(language, expression);
        }

        public static ProjectedColumns ProjectColumns(QueryLanguage language, Expression expression, IEnumerable<ColumnDeclaration> existingColumns, TableAlias newAlias, IEnumerable<TableAlias> existingAliases)
        {
            ColumnProjector projector = new ColumnProjector(language, expression, existingColumns, newAlias, existingAliases);
            Expression expr = projector.Visit(expression);
            return new ProjectedColumns(expr, projector.columns.AsReadOnly());
        }

        public static ProjectedColumns ProjectColumns(QueryLanguage language, Expression expression, IEnumerable<ColumnDeclaration> existingColumns, TableAlias newAlias, params TableAlias[] existingAliases)
        {
            return ProjectColumns(language, expression, existingColumns, newAlias, (IEnumerable<TableAlias>)existingAliases);
        }

        protected override Expression Visit(Expression expression)
        {
            if (this.candidates.Contains(expression))
            {
                if (expression.NodeType == (ExpressionType)DbExpressionType.Column)
                {
                    ColumnExpression column = (ColumnExpression)expression;
                    ColumnExpression mapped;
                    if (this.map.TryGetValue(column, out mapped))
                    {
                        return mapped;
                    }
                    // 检查已经指向了此列的列
                    foreach (ColumnDeclaration existingColumn in this.columns)
                    {
                        ColumnExpression cex = existingColumn.Expression as ColumnExpression;
                        if (cex != null && cex.Alias == column.Alias && cex.Name == column.Name)
                        {
                            return new ColumnExpression(column.Type, column.QueryType, this.newAlias, existingColumn.Name);
                        }
                    }
                    if (this.existingAliases.Contains(column.Alias)) 
                    {
                        int ordinal = this.columns.Count;
                        string columnName = this.GetUniqueColumnName(column.Name);
                        this.columns.Add(new ColumnDeclaration(columnName, column, column.QueryType));
                        mapped = new ColumnExpression(column.Type, column.QueryType, this.newAlias, columnName);
                        this.map.Add(column, mapped);
                        this.columnNames.Add(columnName);
                        return mapped;
                    }
                    return column;
                }
                else
                {
                    string columnName = this.GetNextColumnName();
                    var colType = this.language.TypeSystem.GetColumnType(expression.Type);
                    this.columns.Add(new ColumnDeclaration(columnName, expression, colType));
                    return new ColumnExpression(expression.Type, colType, this.newAlias, columnName);
                }
            }
            else
            {
                return base.Visit(expression);
            }
        }

        private bool IsColumnNameInUse(string name)
        {
            return this.columnNames.Contains(name);
        }

        private string GetUniqueColumnName(string name)
        {
            string baseName = name;
            int suffix = 1;
            while (this.IsColumnNameInUse(name))
            {
                name = baseName + (suffix++);
            }
            return name;
        }

        private string GetNextColumnName()
        {
            return this.GetUniqueColumnName("c" + (iColumn++));
        }

        /// <summary>
        ///在表达式的最后，确定表达式中SelectExpression的列表
        /// </summary>
        class Nominator : DbExpressionVisitor
        {
            QueryLanguage language;
            bool isBlocked;
            HashSet<Expression> candidates;

            private Nominator(QueryLanguage language)
            {
                this.language = language;
                this.candidates = new HashSet<Expression>();
                this.isBlocked = false;
            }

            internal static HashSet<Expression> Nominate(QueryLanguage language, Expression expression)
            {
                Nominator nominator = new Nominator(language);
                nominator.Visit(expression);
                return nominator.candidates;
            }

            protected override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveIsBlocked = this.isBlocked;
                    this.isBlocked = false;
                    if (this.language.MustBeColumn(expression))
                    {
                        this.candidates.Add(expression);
                    }
                    else
                    {
                        base.Visit(expression);
                        if (!this.isBlocked)
                        {
                            if (this.language.CanBeColumn(expression))
                            {
                                this.candidates.Add(expression);
                            }
                            else 
                            {
                                this.isBlocked = true;
                            }
                        }
                        this.isBlocked |= saveIsBlocked;
                    }
                }
                return expression;
            }

            protected override Expression VisitProjection(ProjectionExpression proj)
            {
                this.Visit(proj.Projector);
                return proj;
            }
        }
    }
}
