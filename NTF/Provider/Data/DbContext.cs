using NTF.Data;
using NTF.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NTF.Provider.Data
{

    /// <summary>
    /// 查询数据库的数据源,
    /// 实现了<see cref="IDbContext{T}"/>和<see cref="Query{T}"/>接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbContenxt<T> : Query<T>, IDbContext<T>, IHaveMappingEntity
    {
        MappingEntity entity;
        QueryProvider provider;

        public DbContenxt(Func<string, QueryProvider> provider)
            : base(provider(typeof(T).Namespace), typeof(IDbContext<T>))
        {
            this.provider = provider(Named);
            this.entity = this.provider.Mapping.GetEntity(typeof(T));
        }
        //public DbContenxt(QueryProvider provider, MappingEntity entity)
        //    : base(provider, typeof(IDbContext<T>))
        //{
        //    this.provider = provider;
        //    this.entity = entity;
        //}
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

        public string Named
        {
            get
            {
                return typeof(T).Namespace;
            }
        }

        public T GetById(object id)
        {
            var dbProvider = this.Provider;
            if (dbProvider != null)
            {
                IEnumerable<object> keys = id as IEnumerable<object>;
                if (keys == null)
                    keys = new object[] { id };
                Expression query = ((QueryProvider)dbProvider).Mapping.GetPrimaryKeyQuery(this.entity, this.Expression, keys.Select(v => Expression.Constant(v)).ToArray());
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

}
