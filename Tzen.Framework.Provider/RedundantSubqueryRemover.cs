using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tzen.Framework.Provider
{
    internal class RedundantSubqueryRemover : DbExpressionVisitor
    {
        private RedundantSubqueryRemover() 
        {
        }

        internal static Expression Remove(Expression expression)
        {
            expression = new RedundantSubqueryRemover().Visit(expression);
            expression = SubqueryMerger.Merge(expression);
            return expression;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);

            // first remove all purely redundant subqueries
            List<SelectExpression> redundant = RedundantSubqueryGatherer.Gather(select.From);
            if (redundant != null)
            {
                select = SubqueryRemover.Remove(select, redundant);
            }

            return select;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            proj = (ProjectionExpression)base.VisitProjection(proj);
            if (proj.Source.From is SelectExpression) {
                List<SelectExpression> redundant = RedundantSubqueryGatherer.Gather(proj.Source);
                if (redundant != null) 
                {
                    proj = SubqueryRemover.Remove(proj, redundant);
                }
            }
            return proj;
        }

        internal static bool IsSimpleProjection(SelectExpression select)
        {
            foreach (ColumnDeclaration decl in select.Columns)
            {
                ColumnExpression col = decl.Expression as ColumnExpression;
                if (col == null || decl.Name != col.Name)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsNameMapProjection(SelectExpression select)
        {
            SelectExpression fromSelect = select.From as SelectExpression;
            if (fromSelect == null || select.Columns.Count != fromSelect.Columns.Count)
                return false;
            // test that all columns in 'select' are refering to columns in the same position
            // in 'fromSelect'.
            for (int i = 0, n = select.Columns.Count; i < n; i++)
            {
                ColumnExpression col = select.Columns[i].Expression as ColumnExpression;
                if (col == null || !(col.Name == fromSelect.Columns[i].Name))
                    return false;
            }
            return true;
        }

        internal static bool IsInitialProjection(SelectExpression select)
        {
            return select.From is TableExpression;
        }

        class RedundantSubqueryGatherer : DbExpressionVisitor
        {
            List<SelectExpression> redundant;

            private RedundantSubqueryGatherer()
            {
            }

            internal static List<SelectExpression> Gather(Expression source)
            {
                RedundantSubqueryGatherer gatherer = new RedundantSubqueryGatherer();
                gatherer.Visit(source);
                return gatherer.redundant;
            }

            private static bool IsRedudantSubquery(SelectExpression select)
            {
                return (select.From is SelectExpression || select.From is TableExpression)
                    && (IsSimpleProjection(select) || IsNameMapProjection(select))
                    && !select.IsDistinct
                    && select.Take == null
                    && select.Skip == null
                    && select.Where == null
                    && (select.OrderBy == null || select.OrderBy.Count == 0)
                    && (select.GroupBy == null || select.GroupBy.Count == 0);
            }

            protected override Expression VisitSelect(SelectExpression select)
            {
                if (IsRedudantSubquery(select))
                {
                    if (this.redundant == null)
                    {
                        this.redundant = new List<SelectExpression>();
                    }
                    this.redundant.Add(select);
                }
                return select;
            }

            protected override Expression VisitSubquery(SubqueryExpression subquery)
            {
                // don't gather inside scalar & exists
                return subquery;
            }
        }

        class AggregateChecker : DbExpressionVisitor 
        {
            bool hasAggregate = false;
            private AggregateChecker()
            {
            }

            internal static bool HasAggregates(Expression expression)
            {
                AggregateChecker checker = new AggregateChecker();
                checker.Visit(expression);
                return checker.hasAggregate;
            }

            protected override Expression VisitAggregate(AggregateExpression aggregate)
            {
                this.hasAggregate = true;
                return aggregate;
            }

            protected override Expression VisitSelect(SelectExpression select)
            {
                // only consider aggregates in these locations
                this.Visit(select.Where);
                this.VisitOrderBy(select.OrderBy);
                this.VisitColumnDeclarations(select.Columns);
                return select;
            }

            protected override Expression VisitSubquery(SubqueryExpression subquery)
            {
                // don't count aggregates in subqueries
                return subquery;
            }
        }

        internal class SubqueryMerger : DbExpressionVisitor
        {
            private SubqueryMerger()
            {
            }

            internal static Expression Merge(Expression expression)
            {
                return new SubqueryMerger().Visit(expression);
            }

            bool isTopLevel = true;

            protected override Expression VisitSelect(SelectExpression select)
            {
                bool wasTopLevel = isTopLevel;
                isTopLevel = false;

                select = (SelectExpression)base.VisitSelect(select);

                // next attempt to merge subqueries that would have been removed by the above
                // logic except for the existence of a where clause
                while (CanMergeWithFrom(select, wasTopLevel))
                {
                    SelectExpression fromSelect = (SelectExpression)select.From;

                    // remove the redundant subquery
                    select = SubqueryRemover.Remove(select, fromSelect);

                    // merge where expressions 
                    Expression where = select.Where;
                    if (fromSelect.Where != null)
                    {
                        if (where != null)
                        {
                            where = Expression.And(fromSelect.Where, where);
                        }
                        else
                        {
                            where = fromSelect.Where;
                        }
                    }
                    var orderBy = select.OrderBy != null && select.OrderBy.Count > 0 ? select.OrderBy : fromSelect.OrderBy;
                    var groupBy = select.GroupBy != null && select.GroupBy.Count > 0 ? select.GroupBy : fromSelect.GroupBy;
                    Expression skip = select.Skip != null ? select.Skip : fromSelect.Skip;
                    Expression take = select.Take != null ? select.Take : fromSelect.Take;
                    bool isDistinct = select.IsDistinct | fromSelect.IsDistinct;

                    if (where != select.Where
                        || orderBy != select.OrderBy
                        || groupBy != select.GroupBy
                        || isDistinct != select.IsDistinct
                        || skip != select.Skip
                        || take != select.Take)
                    {
                        select = new SelectExpression(select.Type, select.Alias, select.Columns, select.From, where, orderBy, groupBy, isDistinct, skip, take);
                    }
                }

                return select;
            }

            private static bool CanMergeWithFrom(SelectExpression select, bool isTopLevel)
            {
                SelectExpression fromSelect = select.From as SelectExpression;
                if (fromSelect == null)
                    return false;
                bool frmHasSimpleProjection = IsSimpleProjection(fromSelect);
                bool frmHasNameMapProjection = IsNameMapProjection(fromSelect);
                if (!(frmHasSimpleProjection || frmHasNameMapProjection))
                    return false;
                bool selHasNameMapProjection = IsNameMapProjection(select);
                bool selHasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
                bool selHasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
                bool selHasAggregates = AggregateChecker.HasAggregates(select);
                bool frmHasOrderBy = fromSelect.OrderBy != null && fromSelect.OrderBy.Count > 0;
                bool frmHasGroupBy = fromSelect.GroupBy != null && fromSelect.GroupBy.Count > 0;
                // both cannot have orderby
                if (selHasOrderBy && frmHasOrderBy)
                    return false;
                // both cannot have groupby
                if (selHasOrderBy && frmHasOrderBy)
                    return false;
                // cannot move forward order-by if outer has group-by
                if (frmHasOrderBy && (selHasGroupBy || selHasAggregates || select.IsDistinct))
                    return false;
                // cannot move forward group-by if outer has where clause
                if (frmHasGroupBy && (select.Where != null))
                    return false;
                // cannot move forward a take if outer has take or skip or distinct
                if (fromSelect.Take != null && (select.Take != null || select.Skip != null || select.IsDistinct || selHasAggregates || selHasGroupBy))
                    return false;
                // cannot move forward a skip if outer has skip or distinct
                if (fromSelect.Skip != null && (select.Skip != null || select.IsDistinct || selHasAggregates || selHasGroupBy))
                    return false;
                // cannot move forward a distinct if outer has take, skip, groupby or a different projection
                if (fromSelect.IsDistinct && (select.Take != null || select.Skip != null || !selHasNameMapProjection || selHasGroupBy || selHasAggregates || (selHasOrderBy && !isTopLevel)))
                    return false;
                return true;
            }
        }
    }
}