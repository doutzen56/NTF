using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Tzen.Framework.Provider
{
    internal class SkipRewriter : DbExpressionVisitor
    {
        int aliasCount;

        private SkipRewriter()
        {
        }

        internal static Expression Rewrite(Expression expression)
        {
            return new SkipRewriter().Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);
            if (select.Skip != null)
            {
                SelectExpression newSelect = select.SetSkip(null).SetTake(null);
                bool canAddColumn = !select.IsDistinct && (select.GroupBy == null || select.GroupBy.Count == 0);
                if (!canAddColumn)
                {
                    newSelect = newSelect.AddRedundantSelect("s" + (this.aliasCount++));
                }
                newSelect = newSelect.AddColumn(new ColumnDeclaration("ROWNUM", new RowNumberExpression(select.OrderBy)));

                // add layer for WHERE clause that references new rownum column
                newSelect = newSelect.AddRedundantSelect("s" + (this.aliasCount++));
                newSelect = newSelect.RemoveColumn(newSelect.Columns[newSelect.Columns.Count - 1]);

                string newAlias = ((SelectExpression)newSelect.From).Alias;
                ColumnExpression rnCol = new ColumnExpression(typeof(int), newAlias, "ROWNUM");
                Expression where;
                if (select.Take != null)
                {
                    where = new BetweenExpression(rnCol, Expression.Add(select.Skip, Expression.Constant(1)), Expression.Add(select.Skip, select.Take));
                }
                else
                {
                    where = Expression.GreaterThan(rnCol, select.Skip);
                }
                if (newSelect.Where != null)
                {
                    where = Expression.And(newSelect.Where, where);
                }
                newSelect = newSelect.SetWhere(where);

                select = newSelect;
            }
            return select;
        }
    }
}