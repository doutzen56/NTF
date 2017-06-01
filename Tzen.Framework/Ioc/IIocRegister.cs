using System;
using System.Reflection;

namespace Tzen.Framework.Ioc
{
    public interface IIocRegister
    {
        void Register<TType>(LifeStyle lifeStyle = LifeStyle.Singleton)
            where TType : class;
        void Register(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);
        void Register<TType, TImpl>(LifeStyle lifeStyle = LifeStyle.Singleton)
            where TType : class
            where TImpl : class, TType;
        void Register(Type type, Type impl, LifeStyle lifeStyle = LifeStyle.Singleton);
        void AddDefaultRegister(IDefaultRegister reg);
        void RegisterAssembiyByDefault(Assembly assembly, bool excuteInstaller = true);
        bool IsRegistered(Type type);
        bool IsRegistered<TType>();
    }
}
