using System;

namespace Tzen.Framework.Ioc
{
    public interface IIocResolver
    {
        T Resolve<T>();
        T Resolve<T>(Type type);
        object Resolve(Type type);
        bool IsRegistered(Type type);
        bool IsRegistered<TType>();
        void Release(object obj);
    }
}
