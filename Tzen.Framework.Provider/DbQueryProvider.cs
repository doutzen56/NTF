using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Tzen.Framework.Provider
{

    /// <summary>
    /// 支持LINQ查询，生成SQL语句并执行的Provider
    /// </summary>
    public class SQLQueryProvider : NTFProvider 
    {
        DbConnection connection;

        public SQLQueryProvider(DbConnection connection) 
        {
            this.connection = connection;
        }

        public override string GetQueryText(Expression expression) 
        {
            ProjectionExpression projection = this.Translate(expression);
            return QueryFormatter.Format(projection.Source);
        }

        public override object Execute(Expression expression) 
        {
            // 关闭表达式，开始解析
            LambdaExpression lambda = expression as LambdaExpression;
            if (lambda != null) 
                expression = lambda.Body;

            // 将表达式转换到SQL构建组件
            ProjectionExpression projection = this.Translate(expression);

            string commandText = QueryFormatter.Format(projection.Source);
            ReadOnlyCollection<NamedValueExpression> namedValues = NamedValueGatherer.Gather(projection.Source);
            string[] names = namedValues.Select(v => v.Name).ToArray();

            Expression rootQueryable = RootQueryableFinder.Find(expression);
            Expression providerAccess = Expression.Convert(
                Expression.Property(rootQueryable, typeof(IQueryable).GetProperty("Provider")),
                typeof(SQLQueryProvider)
                );

            LambdaExpression projector = ProjectionBuilder.Build(this, projection, providerAccess);
            LambdaExpression eRead = GetReader(projector, projection.Aggregator, true);
            
            if (lambda != null)
            {
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
                object[] values = namedValues.Select(v => v.Value as ConstantExpression).Select(c => c != null ? c.Value : null).ToArray();
                var fnRead = (Func<DbDataReader, object>)eRead.Compile();
                return Execute(commandText, names, values, fnRead);
            }
        }

        public object Execute(string commandText, string[] paramNames, object[] paramValues, Func<DbDataReader, object> fnRead)
        {
            // 创建命令对象（并填写参数）
            DbCommand cmd = this.connection.CreateCommand();
            cmd.CommandText = commandText;
            for (int i = 0, n = paramNames.Length; i < n; i++)
            {
                DbParameter p = cmd.CreateParameter();
                p.ParameterName = paramNames[i];
                p.Value = paramValues[i];
                cmd.Parameters.Add(p);
            }

            // 执行
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
            // 不执行任何基于本身的查询操作
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

        //将DataReader映射到lambda表达式
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
        /// 创建具有Datareader特点的对象
        /// </summary>
        class ProjectionBuilder : DbExpressionVisitor
        {
            SQLQueryProvider provider;
            ProjectionExpression projection;
            Expression providerAccess;
            ParameterExpression dbDataReaderParam;
            Dictionary<string, int> nameMap;

            private ProjectionBuilder(SQLQueryProvider provider, ProjectionExpression projection, Expression providerAccess)
            {
                this.provider = provider;
                this.projection = projection;
                this.providerAccess = providerAccess;
                this.dbDataReaderParam = Expression.Parameter(typeof(DbDataReader), "reader");
                this.nameMap = projection.Source.Columns.Select((c, i) => new { c, i }).ToDictionary(x => x.c.Name, x => x.i);
            }

            internal static LambdaExpression Build(SQLQueryProvider provider, ProjectionExpression projection, Expression providerAccess)
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
                    if (!column.Type.IsValueType || TypeEx.IsNullableType(column.Type)) {
                        defvalue = Expression.Constant(null, column.Type);
                    }
                    else {
                        defvalue = Expression.Constant(Activator.CreateInstance(column.Type), column.Type);
                    }
                    Expression value = Expression.Convert(
                        Expression.Call(typeof(System.Convert), "ChangeType", null,
                            Expression.Call(this.dbDataReaderParam, "GetValue", null, Expression.Constant(iOrdinal)),
                            Expression.Constant(TypeEx.GetNonNullableType(column.Type))
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
                //外部别名
                projection = (ProjectionExpression)OuterParameterizer.Parameterize(this.projection.Source.Alias, projection);

                string commandText = QueryFormatter.Format(projection.Source);
                ReadOnlyCollection<NamedValueExpression> namedValues = NamedValueGatherer.Gather(projection.Source);
                string[] names = namedValues.Select(v => v.Name).ToArray();
                Expression[] values = namedValues.Select(v => Expression.Convert(this.Visit(v.Value), typeof(object))).ToArray();

                LambdaExpression projector = ProjectionBuilder.Build(this.provider, projection, this.providerAccess);
                LambdaExpression eRead = GetReader(projector, projection.Aggregator, true);

                Type resultType = projection.Aggregator != null ? projection.Aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projection.Projector.Type);
                
                return Expression.Convert(
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
            /// 引用外部别名将列转换为特殊的  Name-Value
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

        /// <summary>
        /// 分离一个<see cref="Query{T}"/>对象的子表达式
        /// </summary>
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

                //记录IQueryable生成的第一个子表达式
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
