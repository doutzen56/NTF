using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Tzen.Framework.Provider {
    internal sealed class ProjectedColumns {
        Expression projector;
        ReadOnlyCollection<ColumnDeclaration> columns;
        internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns) {
            this.projector = projector;
            this.columns = columns;
        }
        internal Expression Projector {
            get { return this.projector; }
        }
        internal ReadOnlyCollection<ColumnDeclaration> Columns {
            get { return this.columns; }
        }
    }

    /// <summary>
    /// ColumnProjection is a visitor that splits an expression representing the result of a query into 
    /// two parts, a list of column declarations of expressions that must be evaluated on the server
    /// and a projector expression that describes how to combine the columns back into the result object
    /// </summary>
    internal class ColumnProjector : DbExpressionVisitor {
        Dictionary<ColumnExpression, ColumnExpression> map;
        List<ColumnDeclaration> columns;
        HashSet<string> columnNames;
        HashSet<Expression> candidates;
        string[] existingAliases;
        string newAlias;
        int iColumn;

        private ColumnProjector(Func<Expression, bool> fnCanBeColumn, Expression expression, string newAlias, params string[] existingAliases) {
            this.newAlias = newAlias;
            this.existingAliases = existingAliases;
            this.map = new Dictionary<ColumnExpression, ColumnExpression>();
            this.columns = new List<ColumnDeclaration>();
            this.columnNames = new HashSet<string>();
            this.candidates = Nominator.Nominate(fnCanBeColumn, expression);
        }

        internal static ProjectedColumns ProjectColumns(Func<Expression, bool> fnCanBeColumn, Expression expression, string newAlias, params string[] existingAliases) {
            ColumnProjector projector = new ColumnProjector(fnCanBeColumn, expression, newAlias, existingAliases);
            Expression expr = projector.Visit(expression);
            return new ProjectedColumns(expr, projector.columns.AsReadOnly());
        }

        protected override Expression Visit(Expression expression) {
            if (this.candidates.Contains(expression)) {
                if (expression.NodeType == (ExpressionType)DbExpressionType.Column) {
                    ColumnExpression column = (ColumnExpression)expression;
                    ColumnExpression mapped;
                    if (this.map.TryGetValue(column, out mapped)) {
                        return mapped;
                    }
                    if (this.existingAliases.Contains(column.Alias)) {
                        int ordinal = this.columns.Count;
                        string columnName = this.GetUniqueColumnName(column.Name);
                        this.columns.Add(new ColumnDeclaration(columnName, column));
                        mapped = new ColumnExpression(column.Type, this.newAlias, columnName);
                        this.map[column] = mapped;
                        this.columnNames.Add(columnName);
                        return mapped;
                    }
                    // must be referring to outer scope
                    return column;
                }
                else {
                    string columnName = this.GetNextColumnName();
                    this.columns.Add(new ColumnDeclaration(columnName, expression));
                    return new ColumnExpression(expression.Type, this.newAlias, columnName);
                }
            }
            else {
                return base.Visit(expression);
            }
        }

        private bool IsColumnNameInUse(string name) {
            return this.columnNames.Contains(name);
        }

        private string GetUniqueColumnName(string name) {
            string baseName = name;
            int suffix = 1;
            while (this.IsColumnNameInUse(name)) {
                name = baseName + (suffix++);
            }
            return name;
        }

        private string GetNextColumnName() {
            return this.GetUniqueColumnName("c" + (iColumn++));
        }

        /// <summary>
        /// Nominator is a class that walks an expression tree bottom up, determining the set of 
        /// candidate expressions that are possible columns of a select expression
        /// </summary>
        class Nominator : DbExpressionVisitor {
            Func<Expression, bool> fnCanBeColumn;
            bool isBlocked;
            HashSet<Expression> candidates;

            private Nominator(Func<Expression, bool> fnCanBeColumn) {
                this.fnCanBeColumn = fnCanBeColumn;
                this.candidates = new HashSet<Expression>();
                this.isBlocked = false;
            }

            internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeColumn, Expression expression) {
                Nominator nominator = new Nominator(fnCanBeColumn);
                nominator.Visit(expression);
                return nominator.candidates;
            }

            protected override Expression Visit(Expression expression) {
                if (expression != null) {
                    bool saveIsBlocked = this.isBlocked;
                    this.isBlocked = false;
                    if (expression.NodeType != (ExpressionType)DbExpressionType.Scalar) {
                        base.Visit(expression);
                    }
                    if (!this.isBlocked) {
                        if (this.fnCanBeColumn(expression)) {
                            this.candidates.Add(expression);
                        }
                        else {
                            this.isBlocked = true;
                        }
                    }
                    this.isBlocked |= saveIsBlocked;
                }
                return expression;
            }
        }
    }
}
