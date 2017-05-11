using System;

namespace Tzen.Framwork.Ioc
{
    public interface IIocResolver
    {
        T Resolve<T>();
        T Resolve<T>(Type type);
        object Resolve(Type type);
    }
}
