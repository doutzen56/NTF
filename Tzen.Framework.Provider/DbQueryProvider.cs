using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Tzen.Framework.Provider 
{

    /// <summary>
    /// A LINQ query provider that executes SQL queries over a DbConnection
    /// </summary>
    public class DbQueryProvider : QueryProvider 
    {
        DbConnection connection;
        TextWriter log;

        public DbQueryProvider(DbConnection connection) 
        {
            this.connection = connection;
        }

        public TextWriter Log 
        {
            get { return this.log; }
            set { this.log = value; }
        }

        public override string GetQueryText(Expression expression) 
        {
            ProjectionExpression projection = this.Translate(expression);
            return QueryFormatter.Format(projection.Source);
        }

        public override object Execute(Expression expression) 
        {
            // strip off lambda for now
            LambdaExpression lambda = expression as LambdaExpression;
            if (lambda != null) 
                expression = lambda.Body;

            // translate query into component pieces
            ProjectionExpression projection = this.Translate(expression);

            string commandText = QueryFormatter.Format(projection.Source);
            ReadOnlyCollection<NamedValueExpression> namedValues = NamedValueGatherer.Gather(projection.Source);
            string[] names = namedValues.Select(v => v.Name).ToArray();

            Expression rootQueryable = RootQueryableFinder.Find(expression);
            Expression providerAccess = Expression.Convert(
                Expression.Property(rootQueryable, typeof(IQueryable).GetProperty("Provider")),
                typeof(DbQueryProvider)
                );

            LambdaExpression projector = ProjectionBuilder.Build(this, projection, providerAccess);
            LambdaExpression eRead = GetReader(projector, projection.Aggregator, true);

            // if asked to execute a lambda, produce a function that will execute this query later
            if (lambda != null)
            {
                // call low-level execute directly on supplied DbQueryProvider
                Expression body = Expression.Call(
                    providerAccess, "Execute", null,
                    Expression.Constant(commandText),
                    Expression.Constant(names),
                    Expression.NewArrayInit(typeof(object), namedValues.Select(v => Expression.Convert(v.Value, typeof(object))).ToArray()),
                    eRead
                    );
                body = Expression.Convert(body, expression.Type);
                LambdaExpression fn = Expression.Lambda(lambda.Type, body, lambda.Parameters);
                return fn.Compile();
            }
            else
            {
                // execute right now!
                object[] values = namedValues.Select(v => v.Value as ConstantExpression).Select(c => c != null ? c.Value : null).ToArray();
                var fnRead = (Func<DbDataReader, object>)eRead.Compile();
                return Execute(commandText, names, values, fnRead);
            }
        }

        public object Execute(string commandText, string[] paramNames, object[] paramValues, Func<DbDataReader, object> fnRead)
        {
            if (this.log != null)
            {
                this.log.WriteLine(commandText);
            }

            // create command object (and fill in parameters)
            DbCommand cmd = this.connection.CreateCommand();
            cmd.CommandText = commandText;
            for (int i = 0, n = paramNames.Length; i < n; i++)
            {
                DbParameter p = cmd.CreateParameter();
                p.ParameterName = paramNames[i];
                p.Value = paramValues[i];
                if (this.log != null)
                {
                    this.log.WriteLine("-- @{0} = [{1}]", p.ParameterName, p.Value);
                }
                cmd.Parameters.Add(p);
            }

            // execute & go
            DbDataReader reader = cmd.ExecuteReader();

            return fnRead(reader);
        }

        private ProjectionExpression Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression, CanBeEvaluatedLocally);
            expression = QueryBinder.Bind(this, expression);
            expression = AggregateRewriter.Rewrite(expression);
            expression = UnusedColumnRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);
            expression = OrderByRewriter.Rewrite(expression);
            expression = SkipRewriter.Rewrite(expression);
            expression = OrderByRewriter.Rewrite(expression);
            expression = RedundantSubqueryRemover.Remove(expression);
            expression = Parameterizer.Parameterize(expression);
            return (ProjectionExpression)expression;
        }

        private bool CanBeEvaluatedLocally(Expression expression)
        {
            // any operation on a query can't be done locally
            ConstantExpression cex = expression as ConstantExpression;
            if (cex != null)
            {
                IQueryable query = cex.Value as IQueryable;
                if (query != null && query.Provider == this)
                    return false;
            }
            MethodCallExpression mc = expression as MethodCallExpression;
            if (mc != null &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable)))
            {
                return false;
            }
            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
                return true;
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

        // create a lambda function that will convert a DbDataReader into a projected (and possibly aggregated) result
        private static LambdaExpression GetReader(LambdaExpression fnProjector, LambdaExpression fnAggregator, bool boxReturn)
        {
            ParameterExpression reader = Expression.Parameter(typeof(DbDataReader), "reader");
            Expression body = Expression.New(typeof(ProjectionReader<>).MakeGenericType(fnProjector.Body.Type).GetConstructors()[0], reader, fnProjector);
            if (fnAggregator != null) {
                body = Expression.Invoke(fnAggregator, body);
            }
            if (boxReturn && body.Type != typeof(object)) {
                body = Expression.Convert(body, typeof(object));
            }
            return Expression.Lambda(body, reader);
        }

        /// <summary>
        /// Creates a function that materializes objects from DbDataReader's
        /// </summary>
        class ProjectionBuilder : DbExpressionVisitor
        {
            DbQueryProvider provider;
            ProjectionExpression projection;
            Expression providerAccess;
            ParameterExpression dbDataReaderParam;
            Dictionary<string, int> nameMap;

            private ProjectionBuilder(DbQueryProvider provider, ProjectionExpression projection, Expression providerAccess)
            {
                this.provider = provider;
                this.projection = projection;
                this.providerAccess = providerAccess;
                this.dbDataReaderParam = Expression.Parameter(typeof(DbDataReader), "reader");
                this.nameMap = projection.Source.Columns.Select((c, i) => new { c, i }).ToDictionary(x => x.c.Name, x => x.i);
            }

            internal static LambdaExpression Build(DbQueryProvider provider, ProjectionExpression projection, Expression providerAccess)
            {
                ProjectionBuilder m = new ProjectionBuilder(provider, projection, providerAccess);
                Expression body = m.Visit(projection.Projector);
                return Expression.Lambda(body, m.dbDataReaderParam);
            }

            protected override Expression VisitColumn(ColumnExpression column)
            {
                if (column.Alias == this.projection.Source.Alias)
                {
                    int iOrdinal = this.nameMap[column.Name];

                    Expression defvalue;
                    if (!column.Type.IsValueType || TypeSystem.IsNullableType(column.Type)) {
                        defvalue = Expression.Constant(null, column.Type);
                    }
                    else {
                        defvalue = Expression.Constant(Activator.CreateInstance(column.Type), column.Type);
                    }

                    // this sucks, but since we don't track true SQL types through the query, and ADO throws exception if you
                    // call the wrong accessor, the best we can do is call GetValue and Convert.ChangeType
                    Expression value = Expression.Convert(
                        Expression.Call(typeof(System.Convert), "ChangeType", null,
                            Expression.Call(this.dbDataReaderParam, "GetValue", null, Expression.Constant(iOrdinal)),
                            Expression.Constant(TypeSystem.GetNonNullableType(column.Type))
                            ),
                            column.Type
                        );

                    return Expression.Condition(
                        Expression.Call(this.dbDataReaderParam, "IsDbNull", null, Expression.Constant(iOrdinal)),
                        defvalue, value
                        );
                }
                return column;
            }

            protected override Expression VisitProjection(ProjectionExpression projection)
            {
                projection = (ProjectionExpression)Parameterizer.Parameterize(projection);
                // also convert references to outer alias to named values!  these become SQL parameters too
                projection = (ProjectionExpression)OuterParameterizer.Parameterize(this.projection.Source.Alias, projection);

                string commandText = QueryFormatter.Format(projection.Source);
                ReadOnlyCollection<NamedValueExpression> namedValues = NamedValueGatherer.Gather(projection.Source);
                string[] names = namedValues.Select(v => v.Name).ToArray();
                Expression[] values = namedValues.Select(v => Expression.Convert(this.Visit(v.Value), typeof(object))).ToArray();

                LambdaExpression projector = ProjectionBuilder.Build(this.provider, projection, this.providerAccess);
                LambdaExpression eRead = GetReader(projector, projection.Aggregator, true);

                Type resultType = projection.Aggregator != null ? projection.Aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projection.Projector.Type);

                // return expression that will call Execute(...)
                return Expression.Convert(
                    //Expression.Call(dbProviderParam, "Execute", null, 
                    Expression.Call(providerAccess, "Execute", null, 
                        Expression.Constant(commandText),
                        Expression.Constant(names),
                        Expression.NewArrayInit(typeof(object), values),
                        eRead
                        ),
                    resultType
                    );
            }

            /// <summary>
            /// columns referencing the outer alias are turned into special named-value parameters
            /// </summary>
            class OuterParameterizer : DbExpressionVisitor
            {
                int iParam;
                string outerAlias;

                internal static Expression Parameterize(string outerAlias, Expression expr)
                {
                    OuterParameterizer op = new OuterParameterizer();
                    op.outerAlias = outerAlias;
                    return op.Visit(expr);
                }

                protected override Expression VisitProjection(ProjectionExpression proj)
                {
                    SelectExpression select = (SelectExpression)this.Visit(proj.Source);
                    if (select != proj.Source)
                    {
                        return new ProjectionExpression(select, proj.Projector, proj.Aggregator);
                    }
                    return proj;
                }

                protected override Expression VisitColumn(ColumnExpression column)
                {
                    if (column.Alias == this.outerAlias)
                    {
                        return new NamedValueExpression("n" + (iParam++), column);
                    }
                    return column;
                }
            }
        }

        // attempt to isolate a sub-expression that accesses a Query<T> object
        class RootQueryableFinder : DbExpressionVisitor
        {
            Expression root;
            internal static Expression Find(Expression expression)
            {
                RootQueryableFinder finder = new RootQueryableFinder();
                finder.Visit(expression);
                return finder.root;
            }

            protected override Expression Visit(Expression exp)
            {
                Expression result = base.Visit(exp);

                // remember the first sub-expression that produces an IQueryable
                if (this.root == null && result != null && typeof(IQueryable).IsAssignableFrom(result.Type))
                {
                    this.root = result;
                }

                return result;
            }
        }
    }

    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement> {
        TKey key;
        IEnumerable<TElement> group;

        public Grouping(TKey key, IEnumerable<TElement> group) {
            this.key = key;
            this.group = group;
        }

        public TKey Key {
            get { return this.key; }
        }

        public IEnumerator<TElement> GetEnumerator() {
            return this.group.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.group.GetEnumerator();
        }
    }
}
