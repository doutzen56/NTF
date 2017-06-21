using NTF.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NTF.Data.Common
{
    /// <summary>
    /// 实体抽象基类
    /// </summary>
    public abstract class MappingEntity
    {
        public abstract string TableName { get; }
        public abstract Type ElementType { get; }
        public abstract Type EntityType { get; }
    }
    /// <summary>
    /// 实体对象
    /// </summary>
    public struct EntityInfo
    {
        object instance;
        MappingEntity mapping;

        public EntityInfo(object instance, MappingEntity mapping)
        {
            this.instance = instance;
            this.mapping = mapping;
        }

        public object Instance
        {
            get { return this.instance; }
        }

        public MappingEntity Mapping
        {
            get { return this.mapping; }
        }
    }

    public interface IHaveMappingEntity
    {
        MappingEntity Entity { get; }
    }

    /// <summary>
    /// 定义实体映射信息和规则
    /// </summary>
    public abstract class QueryMapping
    {
        /// <summary>
        /// 根据指定类型确定表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual string GetTableName(Type type)
        {
            return type.Name;
        }

        /// <summary>
        /// 获取实体根据指定类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual MappingEntity GetEntity(Type type)
        {
            return this.GetEntity(type, this.GetTableName(type));
        }

        /// <summary>
        /// 指定类型与数据库表之间的映射
        /// </summary>
        /// <param name="elementType">指定的CLR类型</param>
        /// <param name="tableName">数据库表名</param>
        /// <returns></returns>
        public abstract MappingEntity GetEntity(Type elementType, string tableName);

        /// <summary>
        /// 根据<see cref="IQueryable{T}"/>的上下文成员获取实体
        /// </summary>
        /// <param name="contextMember"></param>
        /// <returns></returns>
        public abstract MappingEntity GetEntity(MemberInfo contextMember);
        /// <summary>
        /// 获取所有映射成员集合
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract IEnumerable<MemberInfo> GetMappedMembers(MappingEntity entity);
        /// <summary>
        /// 判断指定成员是否主键
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public abstract bool IsPrimaryKey(MappingEntity entity, MemberInfo member);
        /// <summary>
        /// 获取指定实体的所有主键
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual IEnumerable<MemberInfo> GetPrimaryKeyMembers(MappingEntity entity)
        {
            return this.GetMappedMembers(entity).Where(m => this.IsPrimaryKey(entity, m));
        }

        /// <summary>
        /// 确定属性是否映射为关系映射，即是否关联映射
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public abstract bool IsRelationship(MappingEntity entity, MemberInfo member);

        /// <summary>
        /// 确定关联映射属性是否引用单个实体（相对于集合而言）
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual bool IsSingletonRelationship(MappingEntity entity, MemberInfo member)
        {
            if (!this.IsRelationship(entity, member))
                return false;
            Type ieType = TypeEx.FindIEnumerable(TypeEx.GetMemberType(member));
            return ieType == null;
        }

        /// <summary>
        ///确定<see cref="Expression"/>是否可以本地执行
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual bool CanBeEvaluatedLocally(Expression expression)
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
                 mc.Method.DeclaringType == typeof(Queryable) ||
                 mc.Method.DeclaringType == typeof(NonQuery))
                 )
            {
                return false;
            }
            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
                return true;
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }
        /// <summary>
        /// 获取指定实体中的主键
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public abstract object GetPrimaryKey(MappingEntity entity, object instance);
        /// <summary>
        /// 根据指定<see cref="MappingEntity"/>获取主键表达式
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="source"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public abstract Expression GetPrimaryKeyQuery(MappingEntity entity, Expression source, Expression[] keys);
        /// <summary>
        /// 获取指定类型中相关的所有映射实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public abstract IEnumerable<EntityInfo> GetDependentEntities(MappingEntity entity, object instance);

        public abstract IEnumerable<EntityInfo> GetDependingEntities(MappingEntity entity, object instance);
        public abstract object CloneEntity(MappingEntity entity, object instance);
        public abstract bool IsModified(MappingEntity entity, object instance, object original);

        public abstract QueryMapper CreateMapper(QueryTranslator translator);
    }

    public abstract class QueryMapper
    {
        public abstract QueryMapping Mapping { get; }
        public abstract QueryTranslator Translator { get; }

        /// <summary>
        /// Get a query expression that selects all entities from a table
        /// </summary>
        /// <param name="rowType"></param>
        /// <returns></returns>
        public abstract ProjectionExpression GetQueryExpression(MappingEntity entity);

        /// <summary>
        /// Gets an expression that constructs an entity instance relative to a root.
        /// The root is most often a TableExpression, but may be any other experssion such as
        /// a ConstantExpression.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract EntityExpression GetEntityExpression(Expression root, MappingEntity entity);

        /// <summary>
        /// Get an expression for a mapped property relative to a root expression. 
        /// The root is either a TableExpression or an expression defining an entity instance.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public abstract Expression GetMemberExpression(Expression root, MappingEntity entity, MemberInfo member);

        /// <summary>
        /// Get an expression that represents the insert operation for the specified instance.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance">The instance to insert.</param>
        /// <param name="selector">A lambda expression that computes a return value from the operation.</param>
        /// <returns></returns>
        public abstract Expression GetInsertExpression(MappingEntity entity, Expression instance, LambdaExpression selector);

        /// <summary>
        /// Get an expression that represents the update operation for the specified instance.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance"></param>
        /// <param name="updateCheck"></param>
        /// <param name="selector"></param>
        /// <param name="else"></param>
        /// <returns></returns>
        public abstract Expression GetUpdateExpression(MappingEntity entity, Expression instance, Expression updateCheck, LambdaExpression selector, Expression @else);

        /// <summary>
        /// Get an expression that represents the insert-or-update operation for the specified instance.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance"></param>
        /// <param name="updateCheck"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public abstract Expression GetInsertOrUpdateExpression(MappingEntity entity, Expression instance, LambdaExpression updateCheck, LambdaExpression resultSelector);

        /// <summary>
        /// Get an expression that represents the delete operation for the specified instance.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="instance"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public abstract Expression GetDeleteExpression(MappingEntity entity, Expression instance, LambdaExpression predicate);

        /// <summary>
        /// Recreate the type projection with the additional members included
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fnIsIncluded"></param>
        /// <returns></returns>
        public abstract EntityExpression IncludeMembers(EntityExpression entity, Func<MemberInfo, bool> fnIsIncluded);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool HasIncludedMembers(EntityExpression entity);

        /// <summary>
        /// Apply mapping to a sub query expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression ApplyMapping(Expression expression)
        {
            return QueryBinder.Bind(this, expression);
        }

        /// <summary>
        /// Apply mapping translations to this expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            // convert references to LINQ operators into query specific nodes
            expression = QueryBinder.Bind(this, expression);

            // move aggregate computations so they occur in same select as group-by
            expression = AggregateRewriter.Rewrite(this.Translator.Linguist.Language, expression);

            // do reduction so duplicate association's are likely to be clumped together
            expression = UnusedColumnRemover.Remove(expression);
            expression = RedundantColumnRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);
            expression = RedundantJoinRemover.Remove(expression);

            // convert references to association properties into correlated queries
            var bound = RelationshipBinder.Bind(this, expression);
            if (bound != expression)
            {
                expression = bound;
                // clean up after ourselves! (multiple references to same association property)
                expression = RedundantColumnRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
            }

            // rewrite comparision checks between entities and multi-valued constructs
            expression = ComparisonRewriter.Rewrite(this.Mapping, expression);

            return expression;
        }
    }
}
