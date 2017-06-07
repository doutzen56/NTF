using Castle.Windsor;
using System;

namespace NTF.Ioc
{
    public interface IIocManager : IIocRegister, IIocResolver, IDisposable
    {
        IWindsorContainer IocContainer { get; }
        new bool IsRegistered<TType>();
        new bool IsRegistered(Type type);
    }
}
