using System;
using System.Reflection;

namespace NTF.Ioc
{
    /// <summary>
    /// 注入工具类
    /// </summary>
    public interface IIocRegister
    {
        /// <summary>
        /// 注册目标类型
        /// </summary>
        /// <typeparam name="TType">类型TType</typeparam>
        /// <param name="lifeStyle">生命周期</param>
        void Register<TType>(LifeStyle lifeStyle = LifeStyle.Transient)
            where TType : class;
        /// <summary>
        /// 注册目标类型
        /// </summary>
        /// <param name="type">待注册类型</param>
        /// <param name="lifeStyle">生命周期</param>
        void Register(Type type, LifeStyle lifeStyle = LifeStyle.Transient);
        /// <summary>
        /// 注册目标类型，并设置目标类型TType的具体实现类TImpl
        /// </summary>
        /// <typeparam name="TType">类型TType</typeparam>
        /// <typeparam name="TImpl">类型TImpl</typeparam>
        /// <param name="lifeStyle">生命周期</param>
        void Register<TType, TImpl>(LifeStyle lifeStyle = LifeStyle.Transient)
            where TType : class
            where TImpl : class, TType;
        /// <summary>
        /// 注册目标类型，并设置目标类型type的具体实现类impl
        /// </summary>
        /// <param name="type">类型type</param>
        /// <param name="impl">type实现类型impl</param>
        /// <param name="lifeStyle">生命周期</param>
        void Register(Type type, Type impl, LifeStyle lifeStyle = LifeStyle.Transient);
        /// <summary>
        /// 添加约定注册接口实现类到IocManager容器
        /// </summary>
        /// <param name="reg"></param>
        void AddDefaultRegister(IDefaultRegister reg);
        /// <summary>
        /// 注册当前程序集下所有的约定对象
        /// </summary>
        /// <param name="assembly">当前程序集</param>
        /// <param name="excuteInstaller">是否执行Installer</param>
        void RegisterAssembiyByDefault(Assembly assembly, bool excuteInstaller = true);
        /// <summary>
        /// 判断给定类型是否被注册
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsRegistered(Type type);
        /// <summary>
        /// 判断给定类型是否被注册
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        bool IsRegistered<TType>();
    }
}
