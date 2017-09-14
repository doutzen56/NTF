using Castle.Windsor;
using System;

namespace NTF.Ioc
{
    /// <summary>
    /// Ioc统一管理类
    /// </summary>
    public interface IIocManager : IIocRegister, IIocResolver, IDisposable
    {
        /// <summary>
        /// 依赖注入容器
        /// </summary>
        IWindsorContainer IocContainer { get; }
        /// <summary>
        /// 判断给定类型是否被注册
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        new bool IsRegistered<TType>();
        /// <summary>
        /// 判断给定类型是否被注册
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        new bool IsRegistered(Type type);
    }
}
