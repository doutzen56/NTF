using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Tzen.Framework.Provider
{

    /// <summary>
    /// 自定义表达式的扩展类型
    /// </summary>
    internal enum DbExpressionType
    {
        Table = 1000,
        Column,
        Select,
        Projection,
        Join,
        Aggregate,
        Scalar,
        Exists,
        In,
        Grouping,
        AggregateSubquery,
        IsNull,
        Between,
        RowCount,
        NamedValue
    }

    internal static class DbExpressionExtensions
    {
        internal static bool IsDbExpression(this ExpressionType et)
        {
            return ((int)et) >= 1000;
        }
    }

    /// <summary>
    ///SQL中的Table自定义表达式节点
    /// </summary>
    internal class TableExpression : Expression
    {
        string alias;
        string name;
        internal TableExpression(Type type, string alias, string name)
            : base((ExpressionType)DbExpressionType.Table, type)
        {
            this.alias = alias;
            this.name = name;
        }
        internal string Alias
        {
            get { return this.alias; }
        }
        internal string Name
        {
            get { return this.name; }
        }
    }

    /// <summary>
    /// SQL中的Column自定义表达式节点
    /// </summary>
    internal class ColumnExpression : Expression
    {
        string alias;
        string name;
        internal ColumnExpression(Type type, string alias, string name)
            : base((ExpressionType)DbExpressionType.Column, type)
        {
            this.alias = alias;
            this.name = name;
        }
        internal string Alias
        {
            get { return this.alias; }
        }
        internal string Name
        {
            get { return this.name; }
        }
    }

    /// <summary>
    /// Select表达式中Column声明
    /// </summary>
    internal class ColumnDeclaration
    {
        string name;
        Expression expression;
        internal ColumnDeclaration(string name, Expression expression)
        {
            this.name = name;
            this.expression = expression;
        }
        internal string Name
        {
            get { return this.name; }
        }
        internal Expression Expression
        {
            get { return this.expression; }
        }
    }

    /// <summary>
    /// SQL中OrderBy类型
    /// </summary>
    internal enum OrderType
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// OrderBy自定义节点
    /// </summary>
    internal class OrderExpression
    {
        OrderType orderType;
        Expression expression;
        internal OrderExpression(OrderType orderType, Expression expression)
        {
            this.orderType = orderType;
            this.expression = expression;
        }
        internal OrderType OrderType
        {
            get { return this.orderType; }
        }
        internal Expression Expression
        {
            get { return this.expression; }
        }
    }

    /// <summary>
    /// Select查询表达式节点
    /// </summary>
    internal class SelectExpression : Expression
    {
        string alias;
        ReadOnlyCollection<ColumnDeclaration> columns;
        bool isDistinct;
        Expression from;
        Expression where;
        ReadOnlyCollection<OrderExpression> orderBy;
        ReadOnlyCollection<Expression> groupBy;
        Expression take;
        Expression skip;

        internal SelectExpression(
            Type type,
            string alias,
            IEnumerable<ColumnDeclaration> columns,
            Expression from,
            Expression where,
            IEnumerable<OrderExpression> orderBy,
            IEnumerable<Expression> groupBy,
            bool isDistinct,
            Expression skip,
            Expression take)
            : base((ExpressionType)DbExpressionType.Select, type)
        {
            this.alias = alias;
            this.columns = columns as ReadOnlyCollection<ColumnDeclaration>;
            if (this.columns == null)
            {
                this.columns = new List<ColumnDeclaration>(columns).AsReadOnly();
            }
            this.isDistinct = isDistinct;
            this.from = from;
            this.where = where;
            this.orderBy = orderBy as ReadOnlyCollection<OrderExpression>;
            if (this.orderBy == null && orderBy != null)
            {
                this.orderBy = new List<OrderExpression>(orderBy).AsReadOnly();
            }
            this.groupBy = groupBy as ReadOnlyCollection<Expression>;
            if (this.groupBy == null && groupBy != null)
            {
                this.groupBy = new List<Expression>(groupBy).AsReadOnly();
            }
            this.take = take;
            this.skip = skip;
        }
        internal SelectExpression(
            Type type,
            string alias,
            IEnumerable<ColumnDeclaration> columns,
            Expression from,
            Expression where,
            IEnumerable<OrderExpression> orderBy,
            IEnumerable<Expression> groupBy
            )
            : this(type, alias, columns, from, where, orderBy, groupBy, false, null, null)
        {
        }
        internal SelectExpression(
            Type type, string alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where
            )
            : this(type, alias, columns, from, where, null, null)
        {
        }
        internal string Alias
        {
            get { return this.alias; }
        }
        internal ReadOnlyCollection<ColumnDeclaration> Columns
        {
            get { return this.columns; }
        }
        internal Expression From
        {
            get { return this.from; }
        }
        internal Expression Where
        {
            get { return this.where; }
        }
        internal ReadOnlyCollection<OrderExpression> OrderBy
        {
            get { return this.orderBy; }
        }
        internal ReadOnlyCollection<Expression> GroupBy
        {
            get { return this.groupBy; }
        }
        internal bool IsDistinct
        {
            get { return this.isDistinct; }
        }
        internal Expression Skip
        {
            get { return this.skip; }
        }
        internal Expression Take
        {
            get { return this.take; }
        }
    }

    /// <summary>
    /// 表连接类型
    /// </summary>
    internal enum JoinType
    {
        CrossJoin,
        InnerJoin,
        CrossApply,
        LeftOuter
    }

    /// <summary>
    /// SQL连接表达式节点
    /// </summary>
    internal class JoinExpression : Expression
    {
        JoinType joinType;
        Expression left;
        Expression right;
        Expression condition;
        internal JoinExpression(Type type, JoinType joinType, Expression left, Expression right, Expression condition)
            : base((ExpressionType)DbExpressionType.Join, type)
        {
            this.joinType = joinType;
            this.left = left;
            this.right = right;
            this.condition = condition;
        }
        internal JoinType Join
        {
            get { return this.joinType; }
        }
        internal Expression Left
        {
            get { return this.left; }
        }
        internal Expression Right
        {
            get { return this.right; }
        }
        internal new Expression Condition
        {
            get { return this.condition; }
        }
    }
    /// <summary>
    /// 子查询表达式定义
    /// </summary>
    internal abstract class SubqueryExpression : Expression
    {
        SelectExpression select;
        protected SubqueryExpression(DbExpressionType eType, Type type, SelectExpression select)
            : base((ExpressionType)eType, type)
        {
            System.Diagnostics.Debug.Assert(eType == DbExpressionType.Scalar || eType == DbExpressionType.Exists || eType == DbExpressionType.In);
            this.select = select;
        }
        internal SelectExpression Select
        {
            get { return this.select; }
        }
    }

    internal class ScalarExpression : SubqueryExpression
    {
        internal ScalarExpression(Type type, SelectExpression select)
            : base(DbExpressionType.Scalar, type, select)
        {
        }
    }

    internal class ExistsExpression : SubqueryExpression
    {
        internal ExistsExpression(SelectExpression select)
            : base(DbExpressionType.Exists, typeof(bool), select)
        {
        }
    }

    internal class InExpression : SubqueryExpression
    {
        Expression expression;
        ReadOnlyCollection<Expression> values;  // either select or expressions are assigned
        internal InExpression(Expression expression, SelectExpression select)
            : base(DbExpressionType.In, typeof(bool), select)
        {
            this.expression = expression;
        }
        internal InExpression(Expression expression, IEnumerable<Expression> values)
            : base(DbExpressionType.In, typeof(bool), null)
        {
            this.expression = expression;
            this.values = values as ReadOnlyCollection<Expression>;
            if (this.values == null && values != null)
            {
                this.values = new List<Expression>(values).AsReadOnly();
            }
        }
        internal Expression Expression
        {
            get { return this.expression; }
        }
        internal ReadOnlyCollection<Expression> Values
        {
            get { return this.values; }
        }
    }

    internal enum AggregateType
    {
        Count,
        Min,
        Max,
        Sum,
        Average
    }
    /// <summary>
    /// 聚合函数
    /// </summary>
    internal class AggregateExpression : Expression
    {
        AggregateType aggType;
        Expression argument;
        bool isDistinct;
        internal AggregateExpression(Type type, AggregateType aggType, Expression argument, bool isDistinct)
            : base((ExpressionType)DbExpressionType.Aggregate, type)
        {
            this.aggType = aggType;
            this.argument = argument;
            this.isDistinct = isDistinct;
        }
        internal AggregateType AggregateType
        {
            get { return this.aggType; }
        }
        internal Expression Argument
        {
            get { return this.argument; }
        }
        internal bool IsDistinct
        {
            get { return this.isDistinct; }
        }
    }
    /// <summary>
    /// 聚合函数子查询
    /// </summary>
    internal class AggregateSubqueryExpression : Expression
    {
        string groupByAlias;
        Expression aggregateInGroupSelect;
        ScalarExpression aggregateAsSubquery;
        internal AggregateSubqueryExpression(string groupByAlias, Expression aggregateInGroupSelect, ScalarExpression aggregateAsSubquery)
            : base((ExpressionType)DbExpressionType.AggregateSubquery, aggregateAsSubquery.Type)
        {
            this.aggregateInGroupSelect = aggregateInGroupSelect;
            this.groupByAlias = groupByAlias;
            this.aggregateAsSubquery = aggregateAsSubquery;
        }
        internal string GroupByAlias { get { return this.groupByAlias; } }
        internal Expression AggregateInGroupSelect { get { return this.aggregateInGroupSelect; } }
        internal ScalarExpression AggregateAsSubquery { get { return this.aggregateAsSubquery; } }
    }

    /// <summary>
    /// 标识是否可以赋值为null
    /// </summary>
    internal class IsNullExpression : Expression
    {
        Expression expression;
        internal IsNullExpression(Expression expression)
            : base((ExpressionType)DbExpressionType.IsNull, typeof(bool))
        {
            this.expression = expression;
        }
        internal Expression Expression
        {
            get { return this.expression; }
        }
    }

    internal class BetweenExpression : Expression
    {
        Expression expression;
        Expression lower;
        Expression upper;
        internal BetweenExpression(Expression expression, Expression lower, Expression upper)
            : base((ExpressionType)DbExpressionType.Between, expression.Type)
        {
            this.expression = expression;
            this.lower = lower;
            this.upper = upper;
        }
        internal Expression Expression
        {
            get { return this.expression; }
        }
        internal Expression Lower
        {
            get { return this.lower; }
        }
        internal Expression Upper
        {
            get { return this.upper; }
        }
    }

    internal class RowNumberExpression : Expression
    {
        ReadOnlyCollection<OrderExpression> orderBy;
        internal RowNumberExpression(IEnumerable<OrderExpression> orderBy)
            : base((ExpressionType)DbExpressionType.RowCount, typeof(int))
        {
            this.orderBy = orderBy as ReadOnlyCollection<OrderExpression>;
            if (this.orderBy == null && orderBy != null)
            {
                this.orderBy = new List<OrderExpression>(orderBy).AsReadOnly();
            }
        }
        internal ReadOnlyCollection<OrderExpression> OrderBy
        {
            get { return this.orderBy; }
        }
    }

    internal class NamedValueExpression : Expression
    {
        string name;
        Expression value;
        internal NamedValueExpression(string name, Expression value)
            : base((ExpressionType)DbExpressionType.NamedValue, value.Type)
        {
            this.name = name;
            this.value = value;
        }
        internal string Name
        {
            get { return this.name; }
        }
        internal Expression Value
        {
            get { return this.value; }
        }
    }

    /// <summary>
    /// SQL构建组件
    /// </summary>
    /// <remarks>
    /// 从<see cref="SelectExpression"/>表达式构造一个或多个结果对象
    /// </remarks>
    internal class ProjectionExpression : Expression
    {
        SelectExpression source;
        Expression projector;
        LambdaExpression aggregator;
        internal ProjectionExpression(SelectExpression source, Expression projector)
            : this(source, projector, null)
        {
        }
        internal ProjectionExpression(SelectExpression source, Expression projector, LambdaExpression aggregator)
            : base((ExpressionType)DbExpressionType.Projection, aggregator != null ? aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projector.Type))
        {
            this.source = source;
            this.projector = projector;
            this.aggregator = aggregator;
        }
        internal SelectExpression Source
        {
            get { return this.source; }
        }
        internal Expression Projector
        {
            get { return this.projector; }
        }
        internal LambdaExpression Aggregator
        {
            get { return this.aggregator; }
        }
    }

    /// <summary>
    /// 自定义<see cref="ExpressionVisitor"/>扩展
    /// </summary>
    internal class DbExpressionVisitor : ExpressionVisitor
    {
        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            switch ((DbExpressionType)exp.NodeType)
            {
                case DbExpressionType.Table:
                    return this.VisitTable((TableExpression)exp);
                case DbExpressionType.Column:
                    return this.VisitColumn((ColumnExpression)exp);
                case DbExpressionType.Select:
                    return this.VisitSelect((SelectExpression)exp);
                case DbExpressionType.Join:
                    return this.VisitJoin((JoinExpression)exp);
                case DbExpressionType.Aggregate:
                    return this.VisitAggregate((AggregateExpression)exp);
                case DbExpressionType.Scalar:
                case DbExpressionType.Exists:
                case DbExpressionType.In:
                    return this.VisitSubquery((SubqueryExpression)exp);
                case DbExpressionType.AggregateSubquery:
                    return this.VisitAggregateSubquery((AggregateSubqueryExpression)exp);
                case DbExpressionType.IsNull:
                    return this.VisitIsNull((IsNullExpression)exp);
                case DbExpressionType.Between:
                    return this.VisitBetween((BetweenExpression)exp);
                case DbExpressionType.RowCount:
                    return this.VisitRowNumber((RowNumberExpression)exp);
                case DbExpressionType.Projection:
                    return this.VisitProjection((ProjectionExpression)exp);
                case DbExpressionType.NamedValue:
                    return this.VisitNamedValue((NamedValueExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }
        protected virtual Expression VisitTable(TableExpression table)
        {
            return table;
        }
        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            return column;
        }
        protected virtual Expression VisitSelect(SelectExpression select)
        {
            Expression from = this.VisitSource(select.From);
            Expression where = this.Visit(select.Where);
            ReadOnlyCollection<OrderExpression> orderBy = this.VisitOrderBy(select.OrderBy);
            ReadOnlyCollection<Expression> groupBy = this.VisitExpressionList(select.GroupBy);
            Expression skip = this.Visit(select.Skip);
            Expression take = this.Visit(select.Take);
            ReadOnlyCollection<ColumnDeclaration> columns = this.VisitColumnDeclarations(select.Columns);
            if (from != select.From
                || where != select.Where
                || orderBy != select.OrderBy
                || groupBy != select.GroupBy
                || take != select.Take
                || skip != select.Skip
                || columns != select.Columns
                )
            {
                return new SelectExpression(select.Type, select.Alias, columns, from, where, orderBy, groupBy, select.IsDistinct, skip, take);
            }
            return select;
        }
        protected virtual Expression VisitJoin(JoinExpression join)
        {
            Expression left = this.VisitSource(join.Left);
            Expression right = this.VisitSource(join.Right);
            Expression condition = this.Visit(join.Condition);
            if (left != join.Left || right != join.Right || condition != join.Condition)
            {
                return new JoinExpression(join.Type, join.Join, left, right, condition);
            }
            return join;
        }
        protected virtual Expression VisitAggregate(AggregateExpression aggregate)
        {
            Expression arg = this.Visit(aggregate.Argument);
            if (arg != aggregate.Argument)
            {
                return new AggregateExpression(aggregate.Type, aggregate.AggregateType, arg, aggregate.IsDistinct);
            }
            return aggregate;
        }
        protected virtual Expression VisitIsNull(IsNullExpression isnull)
        {
            Expression expr = this.Visit(isnull.Expression);
            if (expr != isnull.Expression)
            {
                return new IsNullExpression(expr);
            }
            return isnull;
        }
        protected virtual Expression VisitBetween(BetweenExpression between)
        {
            Expression expr = this.Visit(between.Expression);
            Expression lower = this.Visit(between.Lower);
            Expression upper = this.Visit(between.Upper);
            if (expr != between.Expression || lower != between.Lower || upper != between.Upper)
            {
                return new BetweenExpression(expr, lower, upper);
            }
            return between;
        }
        protected virtual Expression VisitRowNumber(RowNumberExpression rowNumber)
        {
            var orderby = this.VisitOrderBy(rowNumber.OrderBy);
            if (orderby != rowNumber.OrderBy)
            {
                return new RowNumberExpression(orderby);
            }
            return rowNumber;
        }
        protected virtual Expression VisitNamedValue(NamedValueExpression value)
        {
            return value;
        }
        protected virtual Expression VisitSubquery(SubqueryExpression subquery)
        {
            switch ((DbExpressionType)subquery.NodeType)
            {
                case DbExpressionType.Scalar:
                    return this.VisitScalar((ScalarExpression)subquery);
                case DbExpressionType.Exists:
                    return this.VisitExists((ExistsExpression)subquery);
                case DbExpressionType.In:
                    return this.VisitIn((InExpression)subquery);
            }
            return subquery;
        }

        protected virtual Expression VisitScalar(ScalarExpression scalar)
        {
            SelectExpression select = (SelectExpression)this.Visit(scalar.Select);
            if (select != scalar.Select)
            {
                return new ScalarExpression(scalar.Type, select);
            }
            return scalar;
        }

        protected virtual Expression VisitExists(ExistsExpression exists)
        {
            SelectExpression select = (SelectExpression)this.Visit(exists.Select);
            if (select != exists.Select)
            {
                return new ExistsExpression(select);
            }
            return exists;
        }

        protected virtual Expression VisitIn(InExpression @in)
        {
            Expression expr = this.Visit(@in.Expression);
            if (@in.Select != null)
            {
                SelectExpression select = (SelectExpression)this.Visit(@in.Select);
                if (expr != @in.Expression || select != @in.Select)
                {
                    return new InExpression(expr, select);
                }
            }
            else
            {
                IEnumerable<Expression> values = this.VisitExpressionList(@in.Values);
                if (expr != @in.Expression || values != @in.Values)
                {
                    return new InExpression(expr, values);
                }
            }
            return @in;
        }

        protected virtual Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
        {
            Expression e = this.Visit(aggregate.AggregateAsSubquery);
            System.Diagnostics.Debug.Assert(e is ScalarExpression);
            ScalarExpression subquery = (ScalarExpression)e;
            if (subquery != aggregate.AggregateAsSubquery)
            {
                return new AggregateSubqueryExpression(aggregate.GroupByAlias, aggregate.AggregateInGroupSelect, subquery);
            }
            return aggregate;
        }
        protected virtual Expression VisitSource(Expression source)
        {
            return this.Visit(source);
        }
        protected virtual Expression VisitProjection(ProjectionExpression proj)
        {
            SelectExpression source = (SelectExpression)this.Visit(proj.Source);
            Expression projector = this.Visit(proj.Projector);
            if (source != proj.Source || projector != proj.Projector)
            {
                return new ProjectionExpression(source, projector, proj.Aggregator);
            }
            return proj;
        }
        protected ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            List<ColumnDeclaration> alternate = null;
            for (int i = 0, n = columns.Count; i < n; i++)
            {
                ColumnDeclaration column = columns[i];
                Expression e = this.Visit(column.Expression);
                if (alternate == null && e != column.Expression)
                {
                    alternate = columns.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(new ColumnDeclaration(column.Name, e));
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return columns;
        }
        protected ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> expressions)
        {
            if (expressions != null)
            {
                List<OrderExpression> alternate = null;
                for (int i = 0, n = expressions.Count; i < n; i++)
                {
                    OrderExpression expr = expressions[i];
                    Expression e = this.Visit(expr.Expression);
                    if (alternate == null && e != expr.Expression)
                    {
                        alternate = expressions.Take(i).ToList();
                    }
                    if (alternate != null)
                    {
                        alternate.Add(new OrderExpression(expr.OrderType, e));
                    }
                }
                if (alternate != null)
                {
                    return alternate.AsReadOnly();
                }
            }
            return expressions;
        }
    }
}
