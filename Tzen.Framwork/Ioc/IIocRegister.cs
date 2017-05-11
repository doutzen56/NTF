using System;
using System.Reflection;

namespace Tzen.Framwork.Ioc
{
    public interface IIocRegister
    {
        void Register<TType>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
            where TType : class;
        void Register(Type type, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton);
        void Register<TType, TImpl>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
            where TType : class
            where TImpl : class, TType;
        void Register(Type type, Type impl, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton);
        void AddConventionReg(IConventionRegister reg);
        void RegisterAssembiyByConvention(Assembly assembly, bool excuteInstaller = true);
    }
}
