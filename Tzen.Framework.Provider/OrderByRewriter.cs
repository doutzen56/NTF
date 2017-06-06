using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Tzen.Framework.Provider
{
    /// <summary>
    /// 将OrderBy移动到最外层Select
    /// </summary>
    internal class OrderByRewriter : DbExpressionVisitor {
        IList<OrderExpression> gatheredOrderings;
        HashSet<string> uniqueColumns;
        bool isOuterMostSelect;

        private OrderByRewriter() {
            this.isOuterMostSelect = true;
        }

        internal static Expression Rewrite(Expression expression) {
            return new OrderByRewriter().Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select) {
            bool saveIsOuterMostSelect = this.isOuterMostSelect;
            try {
                this.isOuterMostSelect = false;
                select = (SelectExpression)base.VisitSelect(select);

                bool hasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
                bool hasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
                bool canHaveOrderBy = saveIsOuterMostSelect || select.Take != null || select.Skip != null;
                bool canReceiveOrderings = canHaveOrderBy && !hasGroupBy && !select.IsDistinct;

                if (hasOrderBy) {
                    this.PrependOrderings(select.OrderBy);
                }

                IEnumerable<OrderExpression> orderings = null;
                if (canReceiveOrderings) {
                    orderings = this.gatheredOrderings;
                }
                else if (canHaveOrderBy) {
                    orderings = select.OrderBy;
                }
                bool canPassOnOrderings = !saveIsOuterMostSelect && !hasGroupBy && !select.IsDistinct;
                ReadOnlyCollection<ColumnDeclaration> columns = select.Columns;
                if (this.gatheredOrderings != null) {
                    if (canPassOnOrderings) {
                        HashSet<string> producedAliases = AliasesProduced.Gather(select.From);
                        // OrderBy重新绑定
                        BindResult project = this.RebindOrderings(this.gatheredOrderings, select.Alias, producedAliases, select.Columns);
                        this.gatheredOrderings = null;
                        this.PrependOrderings(project.Orderings);
                        columns = project.Columns;
                    }
                    else {
                        this.gatheredOrderings = null;
                    }
                }
                if (orderings != select.OrderBy || columns != select.Columns) {
                    select = new SelectExpression(select.Type, select.Alias, columns, select.From, select.Where, orderings, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
                }
                return select;
            }
            finally {
                this.isOuterMostSelect = saveIsOuterMostSelect;
            }
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery) {
            var saveOrderings = this.gatheredOrderings;
            this.gatheredOrderings = null;
            var result = base.VisitSubquery(subquery);
            this.gatheredOrderings = saveOrderings;
            return result;
        }

        protected override Expression VisitJoin(JoinExpression join) {
            // 确保访问OrderBy右侧表达式的时候，左边的已缓存记录                                                 
            Expression left = this.VisitSource(join.Left);
            IList<OrderExpression> leftOrders = this.gatheredOrderings;
            this.gatheredOrderings = null; // 重置模板
            Expression right = this.VisitSource(join.Right);
            this.PrependOrderings(leftOrders);
            Expression condition = this.Visit(join.Condition);
            if (left != join.Left || right != join.Right || condition != join.Condition) {
                return new JoinExpression(join.Type, join.Join, left, right, condition);
            }
            return join;
        }

        /// <summary>
        /// 在已有序列上添加一个新的序列
        /// </summary>
        /// <param name="newOrderings"></param>
        protected void PrependOrderings(IList<OrderExpression> newOrderings) {
            if (newOrderings != null) {
                if (this.gatheredOrderings == null) {
                    this.gatheredOrderings = new List<OrderExpression>();
                    this.uniqueColumns = new HashSet<string>();
                }                
                for (int i = newOrderings.Count - 1; i >= 0; i--) {
                    var ordering = newOrderings[i];
                    ColumnExpression column = ordering.Expression as ColumnExpression;
                    if (column != null) {
                        string hash = column.Alias + ":" + column.Name;
                        if (!this.uniqueColumns.Contains(hash)) {
                            this.gatheredOrderings.Insert(0, ordering);
                            this.uniqueColumns.Add(hash);
                        }
                    }
                    else {
                        this.gatheredOrderings.Insert(0, ordering);
                    }
                }
            }
        }

        protected class BindResult {
            ReadOnlyCollection<ColumnDeclaration> columns;
            ReadOnlyCollection<OrderExpression> orderings;
            public BindResult(IEnumerable<ColumnDeclaration> columns, IEnumerable<OrderExpression> orderings) {
                this.columns = columns as ReadOnlyCollection<ColumnDeclaration>;
                if (this.columns == null) {
                    this.columns = new List<ColumnDeclaration>(columns).AsReadOnly();
                }
                this.orderings = orderings as ReadOnlyCollection<OrderExpression>;
                if (this.orderings == null) {
                    this.orderings = new List<OrderExpression>(orderings).AsReadOnly();
                }
            }
            public ReadOnlyCollection<ColumnDeclaration> Columns {
                get { return this.columns; }
            }
            public ReadOnlyCollection<OrderExpression> Orderings {
                get { return this.orderings; }
            }
        }

        /// <summary>
        /// 使用列的别名和别名映射重新绑定排序表达式
        /// </summary>
        protected virtual BindResult RebindOrderings(IEnumerable<OrderExpression> orderings, string alias, HashSet<string> existingAliases, IEnumerable<ColumnDeclaration> existingColumns) {
            List<ColumnDeclaration> newColumns = null;
            List<OrderExpression> newOrderings = new List<OrderExpression>();
            foreach (OrderExpression ordering in orderings) {
                Expression expr = ordering.Expression;
                ColumnExpression column = expr as ColumnExpression;
                if (column == null || (existingAliases != null && existingAliases.Contains(column.Alias))) {
                    // 检查已声明的列是否已包含类似类型的表达式
                    int iOrdinal = 0;
                    foreach (ColumnDeclaration decl in existingColumns) {
                        ColumnExpression declColumn = decl.Expression as ColumnExpression;
                        if (decl.Expression == ordering.Expression || 
                            (column != null && declColumn != null && column.Alias == declColumn.Alias && column.Name == declColumn.Name)) {
                            // 如果有，创建一个Column表达式
                            expr = new ColumnExpression(column.Type, alias, decl.Name);
                            break;
                        }
                        iOrdinal++;
                    }
                    // 如果尚未构建，添加一个新的列声明
                    if (expr == ordering.Expression) {
                        if (newColumns == null) {
                            newColumns = new List<ColumnDeclaration>(existingColumns);
                            existingColumns = newColumns;
                        }
                        string colName = column != null ? column.Name : "c" + iOrdinal;
                        newColumns.Add(new ColumnDeclaration(colName, ordering.Expression));
                        expr = new ColumnExpression(expr.Type, alias, colName);
                    }
                    newOrderings.Add(new OrderExpression(ordering.OrderType, expr));
                }
            }
            return new BindResult(existingColumns, newOrderings);
        }
    }
}
