using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Tzen.Framework.Provider
{
    internal static class DbExtensions
    {
        internal static SelectExpression AddColumn(this SelectExpression select, ColumnDeclaration column)
        {
            List<ColumnDeclaration> columns = new List<ColumnDeclaration>(select.Columns);
            columns.Add(column);
            return new SelectExpression(select.Type, select.Alias, columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
        }

        internal static SelectExpression RemoveColumn(this SelectExpression select, ColumnDeclaration column)
        {
            List<ColumnDeclaration> columns = new List<ColumnDeclaration>(select.Columns);
            columns.Remove(column);
            return new SelectExpression(select.Type, select.Alias, columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
        }

        internal static SelectExpression SetDistinct(this SelectExpression select, bool isDistinct)
        {
            if (select.IsDistinct != isDistinct)
            {
                return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, isDistinct, select.Skip, select.Take);
            }
            return select;
        }

        internal static SelectExpression SetWhere(this SelectExpression select, Expression where)
        {
            if (where != select.Where)
            {
                return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
            }
            return select;
        }

        internal static SelectExpression AddOrderExpression(this SelectExpression select, OrderExpression ordering)
        {
            List<OrderExpression> orderby = new List<OrderExpression>();
            if (select.OrderBy != null)
                orderby.AddRange(select.OrderBy);
            orderby.Add(ordering);
            return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, select.Where, orderby, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
        }

        internal static SelectExpression RemoveOrderExpression(this SelectExpression select, OrderExpression ordering)
        {
            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                List<OrderExpression> orderby = new List<OrderExpression>(select.OrderBy);
                orderby.Remove(ordering);
                return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, select.Where, orderby, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
            }
            return select;
        }

        internal static SelectExpression AddGroupExpression(this SelectExpression select, Expression expression)
        {
            List<Expression> groupby = new List<Expression>();
            if (select.GroupBy != null)
                groupby.AddRange(select.GroupBy);
            groupby.Add(expression);
            return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, select.Where, select.OrderBy, groupby, select.IsDistinct, select.Skip, select.Take);
        }

        internal static SelectExpression RemoveGroupExpression(this SelectExpression select, Expression expression)
        {
            if (select.GroupBy != null && select.GroupBy.Count > 0)
            {
                List<Expression> groupby = new List<Expression>(select.GroupBy);
                groupby.Remove(expression);
                return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, select.Where, select.OrderBy, groupby, select.IsDistinct, select.Skip, select.Take);
            }
            return select;
        }

        internal static SelectExpression SetSkip(this SelectExpression select, Expression skip)
        {
            if (skip != select.Skip)
            {
                return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, skip, select.Take);
            }
            return select;
        }

        internal static SelectExpression SetTake(this SelectExpression select, Expression take)
        {
            if (take != select.Take)
            {
                return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, take);
            }
            return select;
        }

        internal static SelectExpression AddRedundantSelect(this SelectExpression select, string newAlias)
        {
            SelectExpression mapped = (SelectExpression)ColumnMapper.Map(AliasesProduced.Gather(select.From), newAlias, select);
            SelectExpression newFrom = new SelectExpression(select.Type, newAlias, select.Columns, select.From, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Skip, select.Take);
            return new SelectExpression(select.Type, select.Alias, mapped.Columns, newFrom, null, null, null, false, null, null);
        }

        internal static SelectExpression RemoveRedundantFrom(this SelectExpression select)
        {
            SelectExpression fromSelect = select.From as SelectExpression;
            if (fromSelect != null)
            {
                return SubqueryRemover.Remove(select, fromSelect);
            }
            return select;
        }

        /// <summary>
        /// Rewrite all column references to one or more alias to a new specific alias
        /// </summary>
        class ColumnMapper : DbExpressionVisitor
        {
            HashSet<string> oldAliases;
            string newAlias;

            private ColumnMapper(IEnumerable<string> oldAliases, string newAlias)
            {
                this.oldAliases = new HashSet<string>(oldAliases);
                this.newAlias = newAlias;
            }

            internal static Expression Map(IEnumerable<string> oldAliases, string newAlias, Expression expression)
            {
                return new ColumnMapper(oldAliases, newAlias).Visit(expression);
            }

            protected override Expression VisitColumn(ColumnExpression column)
            {
                if (this.oldAliases.Contains(column.Alias))
                {
                    return new ColumnExpression(column.Type, this.newAlias, column.Name);
                }
                return column;
            }
        }
    }
}