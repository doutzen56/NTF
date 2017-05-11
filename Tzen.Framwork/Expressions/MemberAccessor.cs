using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Tzen.Framwork.Expressions
{
    public static class MemberAccessor
    {
        private static ConcurrentDictionary<string, Func<object, object[], object>> cache = new ConcurrentDictionary<string, Func<object, object[], object>>();

        public static object Process(MemberExpression e)
        {
            var topMember = GetRootMember(e);
            if (topMember == null)
                throw new InvalidOperationException("需计算的条件表达式只支持由 MemberExpression 和 ConstantExpression 组成的表达式");

            if (topMember.Expression == null)
            {
                var aquire = cache.GetOrAdd(e.ToString(), key => GetStaticProperty(e, topMember));
                return aquire(null, null);
            }
            else
            {
                var aquire = cache.GetOrAdd(e.ToString(), key => GetInstanceProperty(e, topMember));
                return aquire((topMember.Expression as ConstantExpression).Value, null);
            }
        }

        public static TProperty Process<TModel, TProperty>(Expression<Func<TModel, TProperty>> e, TModel instance)
        {
            var aquire = Compiler<TModel, TProperty>.Compile(e);
            return aquire(instance);
        }

        public static object Process<TModel>(TModel instance, MemberInfo m)
        {
            var aquire = ModelCompiler<TModel>.Compile(m);
            return aquire(instance);
        }

        #region 解释SQL查询使用

        public static ConstantExpression GetRootConstant(MemberExpression e)
        {
            if (e.Expression == null)
                return null;

            switch (e.Expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return GetRootConstant(e.Expression as MemberExpression);
                case ExpressionType.Constant:
                    return e.Expression as ConstantExpression;
                default:
                    return null;
            }
        }

        public static MemberExpression GetRootMember(MemberExpression e)
        {
            if (e.Expression == null || e.Expression.NodeType == ExpressionType.Constant)
                return e;

            if (e.Expression.NodeType == ExpressionType.MemberAccess)
                return GetRootMember(e.Expression as MemberExpression);

            return null;
        }

        private static Func<object, object[], object> GetInstanceProperty(Expression e, MemberExpression topMember)
        {
            var parameter = Expression.Parameter(typeof(object), "local");
            var parameters = Expression.Parameter(typeof(object[]), "args");
            var castExpression = Expression.Convert(parameter, topMember.Member.DeclaringType);
            var localExpression = topMember.Update(castExpression);
            var replaceExpression = ExpressionModifier.Replace(e, topMember, localExpression);
            replaceExpression = Expression.Convert(replaceExpression, typeof(object));
            var compileExpression = Expression.Lambda<Func<object, object[], object>>(replaceExpression, parameter, parameters);
            return compileExpression.Compile();
        }

        public static Func<object, object[], object> GetStaticProperty(Expression e, MemberExpression topMember)
        {
            var parameter = Expression.Parameter(typeof(object), "local");
            var parameters = Expression.Parameter(typeof(object[]), "args");
            var convertExpression = Expression.Convert(e, typeof(object));
            var compileExpression = Expression.Lambda<Func<object, object[], object>>(convertExpression, parameter, parameters);
            return compileExpression.Compile();
        }

        #endregion

        #region 强类型访问通用

        private static class Compiler<TModel, TProperty>
        {
            private static readonly ConcurrentDictionary<MemberInfo, Func<TModel, TProperty>> cache = new ConcurrentDictionary<MemberInfo, Func<TModel, TProperty>>();

            public static Func<TModel, TProperty> Compile(Expression<Func<TModel, TProperty>> e)
            {
                var me = e.Body as MemberExpression;
                me.ThrowIfNull("e.Body is not a MemberExpression");
                return cache.GetOrAdd(me.Member, key => e.Compile());
            }
        }

        private static class ModelCompiler<TModel>
        {
            private static readonly ConcurrentDictionary<MemberInfo, Func<TModel, object>> cache = new ConcurrentDictionary<MemberInfo, Func<TModel, object>>();

            public static Func<TModel, object> Compile(MemberInfo m)
            {
                return cache.GetOrAdd(m, (MemberInfo key) =>
                {
                    var parameter = Expression.Parameter(typeof(TModel), "model");
                    var expression = Expression.MakeMemberAccess(parameter, m);
                    var cast = Expression.Convert(expression, typeof(object));
                    var lambda = Expression.Lambda<Func<TModel, object>>(cast, parameter);
                    return lambda.Compile();
                });
            }
        }

        #endregion
    }
}
