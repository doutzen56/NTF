using NTF.Extensions;
using NTF.Data.Common;
using NTF.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NTF.Provider;

namespace NTF.Data
{
    /// <summary>
    /// A LINQ IQueryable query provider that executes database queries over a DbConnection
    /// </summary>
    public abstract class EntityProvider : QueryProvider, IDbContextProvider, ICreateExecutor
    {
        QueryLanguage language;
        QueryMapping mapping;
        QueryPolicy policy;
        TextWriter log;
        Dictionary<MappingEntity, IDbContext> tables;
        QueryCache cache;

        public EntityProvider(QueryLanguage language, QueryMapping mapping, QueryPolicy policy)
        {
            if (language == null)
                throw new InvalidOperationException("Language not specified");
            if (mapping == null)
                throw new InvalidOperationException("Mapping not specified");
            if (policy == null)
                throw new InvalidOperationException("Policy not specified");
            this.language = language;
            this.mapping = mapping;
            this.policy = policy;
            this.tables = new Dictionary<MappingEntity, IDbContext>();
        }

        public QueryMapping Mapping
        {
            get { return this.mapping; }
        }

        public QueryLanguage Language
        {
            get { return this.language; }
        }

        public QueryPolicy Policy
        {
            get { return this.policy; }

            set
            {
                if (value == null)
                {
                    this.policy = QueryPolicy.Default;
                }
                else
                {
                    this.policy = value;
                }
            }
        }

        public TextWriter Log
        {
            get { return this.log; }
            set { this.log = value; }
        }

        public QueryCache Cache
        {
            get { return this.cache; }
            set { this.cache = value; }
        }

        public IDbContext GetTable(MappingEntity entity)
        {
            IDbContext table;
            if (!this.tables.TryGetValue(entity, out table))
            {
                table = this.CreateTable(entity);
                this.tables.Add(entity, table);
            }
            return table;
        }

        protected virtual IDbContext CreateTable(MappingEntity entity)
        {
            return (IDbContext) Activator.CreateInstance(
                typeof(EntityDbContenxt<>).MakeGenericType(entity.ElementType), 
                new object[] { this, entity }
                );
        }

        public virtual IDbContext<T> GetTable<T>()
        {
            return GetDbContext<T>(null);
        }

        public virtual IDbContext<T> GetDbContext<T>(string tableId)
        {
            return (IDbContext<T>)this.GetDbContext(typeof(T), tableId);
        }

        public virtual IDbContext GetTable(Type type)
        {
            return GetDbContext(type, null);
        }

        public virtual IDbContext GetDbContext(Type type, string tableId)
        {
            return this.GetTable(this.Mapping.GetEntity(type, tableId));
        }

        public bool CanBeEvaluatedLocally(Expression expression)
        {
            return this.Mapping.CanBeEvaluatedLocally(expression);
        }

        public virtual bool CanBeParameter(Expression expression)
        {
            Type type = TypeEx.GetNonNullableType(expression.Type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                    if (expression.Type == typeof(Byte[]) ||
                        expression.Type == typeof(Char[]))
                        return true;
                    return false;
                default:
                    return true;
            }
        }

        protected abstract QueryExecutor CreateExecutor();

        QueryExecutor ICreateExecutor.CreateExecutor()
        {
            return this.CreateExecutor();
        }

        public class EntityDbContenxt<T> : Query<T>, IDbContext<T>, IHaveMappingEntity
        {
            MappingEntity entity;
            EntityProvider provider;

            public EntityDbContenxt(EntityProvider provider, MappingEntity entity)
                : base(provider, typeof(IDbContext<T>))
            {
                this.provider = provider;
                this.entity = entity;
            }

            public MappingEntity Entity
            {
                get { return this.entity; }
            }

            new public IDbContextProvider Provider
            {
                get { return this.provider; }
            }

            public string TableName
            {
                get { return this.entity.TableName; }
            }

            public Type EntityType
            {
                get { return this.entity.EntityType; }
            }

            public T GetById(object id)
            {
                var dbProvider = this.Provider;
                if (dbProvider != null)
                {
                    IEnumerable<object> keys = id as IEnumerable<object>;
                    if (keys == null)
                        keys = new object[] { id };
                    Expression query = ((EntityProvider)dbProvider).Mapping.GetPrimaryKeyQuery(this.entity, this.Expression, keys.Select(v => Expression.Constant(v)).ToArray());
                    return this.Provider.Execute<T>(query);
                }
                return default(T);
            }

            object IDbContext.GetById(object id)
            {
                return this.GetById(id);
            }

            public int Insert(T instance)
            {
                return NonQuery.Insert(this, instance);
            }

            int IDbContext.Insert(object instance)
            {
                return this.Insert((T)instance);
            }

            public int Delete(T instance)
            {
                return NonQuery.Delete(this, instance);
            }

            int IDbContext.Delete(object instance)
            {
                return this.Delete((T)instance);
            }

            public int Update(T instance)
            {
                return NonQuery.Update(this, instance);
            }

            int IDbContext.Update(object instance)
            {
                return this.Update((T)instance);
            }
        }

        public override string GetQueryText(Expression expression)
        {
            Expression plan = this.GetExecutionPlan(expression);
            var commands = CommandGatherer.Gather(plan).Select(c => c.CommandText).ToArray();
            return string.Join("\n\n", commands);
        }

        class CommandGatherer : DbExpressionVisitor
        {
            List<QueryCommand> commands = new List<QueryCommand>();

            public static ReadOnlyCollection<QueryCommand> Gather(Expression expression)
            {
                var gatherer = new CommandGatherer();
                gatherer.Visit(expression);
                return gatherer.commands.AsReadOnly();
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {
                QueryCommand qc = c.Value as QueryCommand;
                if (qc != null)
                {
                    this.commands.Add(qc);
                }
                return c;
            }
        }

        public string GetQueryPlan(Expression expression)
        {
            Expression plan = this.GetExecutionPlan(expression);
            return DbExpressionWriter.WriteToString(this.Language, plan);
        }

        protected virtual QueryTranslator CreateTranslator()
        {
            return new QueryTranslator(this.language, this.mapping, this.policy);
        }

        public abstract void DoTransacted(Action action);
        public abstract void DoConnected(Action action);
        public abstract int ExecuteCommand(string commandText);

        /// <summary>
        /// Execute the query expression (does translation, etc.)
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override object Execute(Expression expression)
        {
            LambdaExpression lambda = expression as LambdaExpression;

            if (lambda == null && this.cache != null && expression.NodeType != ExpressionType.Constant)
            {
                return this.cache.Execute(expression);
            }

            Expression plan = this.GetExecutionPlan(expression);

            if (lambda != null)
            {
                // compile & return the execution plan so it can be used multiple times
                LambdaExpression fn = Expression.Lambda(lambda.Type, plan, lambda.Parameters);
#if NOREFEMIT
                    return ExpressionEvaluator.CreateDelegate(fn);
#else
                return fn.Compile();
#endif
            }
            else
            {
                // compile the execution plan and invoke it
                Expression<Func<object>> efn = Expression.Lambda<Func<object>>(Expression.Convert(plan, typeof(object)));
#if NOREFEMIT
                    return ExpressionEvaluator.Eval(efn, new object[] { });
#else
                Func<object> fn = efn.Compile();
                return fn();
#endif
            }
        }

        /// <summary>
        /// Convert the query expression into an execution plan
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression GetExecutionPlan(Expression expression)
        {
            // strip off lambda for now
            LambdaExpression lambda = expression as LambdaExpression;
            if (lambda != null)
                expression = lambda.Body;

            QueryTranslator translator = this.CreateTranslator();

            // translate query into client & server parts
            Expression translation = translator.Translate(expression);

            var parameters = lambda != null ? lambda.Parameters : null;
            Expression provider = this.Find(expression, parameters, typeof(EntityProvider));
            if (provider == null)
            {
                Expression rootQueryable = this.Find(expression, parameters, typeof(IQueryable));
                provider = Expression.Property(rootQueryable, typeof(IQueryable).GetProperty("Provider"));
            }

            return translator.Police.BuildExecutionPlan(translation, provider);
        }

        private Expression Find(Expression expression, IList<ParameterExpression> parameters, Type type)
        {
            if (parameters != null)
            {
                Expression found = parameters.FirstOrDefault(p => type.IsAssignableFrom(p.Type));
                if (found != null)
                    return found;
            }
            return TypedSubtreeFinder.Find(expression, type);
        }
           
        public static QueryMapping GetMapping(string mappingId)
        {
            if (mappingId != null)
            {
                Type type = FindLoadedType(mappingId);
                if (type != null)
                {
                    return new AttributeMapping(type);
                }
                if (File.Exists(mappingId))
                {
                    return XmlMapping.FromXml(File.ReadAllText(mappingId));
                }
            }
            return new ImplicitMapping();
        }

        public static Type GetProviderType(string providerName)
        {
            if (!string.IsNullOrEmpty(providerName))
            {
                var type = FindInstancesIn(typeof(EntityProvider), providerName).FirstOrDefault();
                if (type != null)
                    return type;
            }
            return null;
        }

        private static Type FindLoadedType(string typeName)
        {
            foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assem.GetType(typeName, false, true);
                if (type != null)
                    return type;
            }
            return null;
        }

        private static IEnumerable<Type> FindInstancesIn(Type type, string assemblyName)
        {
            Assembly assembly = GetAssemblyForNamespace(assemblyName);
            if (assembly != null)
            {
                foreach (var atype in assembly.GetTypes())
                {
                    if (string.Compare(atype.Namespace, assemblyName, true) == 0
                        && type.IsAssignableFrom(atype))
                    {
                        yield return atype;
                    }
                }
            }
        }

        private static Assembly GetAssemblyForNamespace(string nspace)
        {
            foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assem.FullName.Contains(nspace))
                {
                    return assem;
                }
            }

            return Load(nspace + ".dll");
        }

        private static Assembly Load(string name)
        {
            // try to load it.
            try
            {
                return Assembly.LoadFrom(name);
            }
            catch
            {
            }
            return null;
        }
    }
}
