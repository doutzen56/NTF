using System;
using System.Reflection;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using System.Collections.Generic;
using Castle.Windsor.Installer;

namespace Tzen.Framwork.Ioc
{
    public class IocManager : IIocManager
    {
        #region IocManager 属性
        /// <summary>
        /// <see cref="IocManager"/>单例对象
        /// </summary>
        public static IocManager Instance { get; private set; }
        /// <summary>
        /// Ioc容器
        /// </summary>
        public IWindsorContainer IocContainer { get; private set; }
        private readonly List<IConventionRegister> _conventionRegster;
        #endregion

        #region IocManager 构造函数
        static IocManager()
        {
            Instance = new IocManager();
        }
        private IocManager()
        {
            //初始化Ioc容器
            IocContainer = new WindsorContainer();
            _conventionRegster = new List<IConventionRegister>();
            //注册自己
            IocContainer.Register(
                Component.For<IocManager, IIocManager, IIocRegister, IIocResolver>().UsingFactoryMethod(() => this)
                );
        }
        #endregion

        #region IIocRegister
        /// <summary>
        /// 注册目标类型
        /// </summary>
        /// <param name="type">待注册类型</param>
        /// <param name="lifeStyle">生命周期</param>
        public void Register(Type type, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
        {
            IocContainer.Register(SetLifeStyle(Component.For(type), lifeStyle));
        }
        /// <summary>
        /// 注册目标类型，并设置目标类型type的具体实现类impl
        /// </summary>
        /// <param name="type">类型type</param>
        /// <param name="impl">type实现类型impl</param>
        /// <param name="lifeStyle">生命周期</param>
        public void Register(Type type, Type impl, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton)
        {
            IocContainer.Register(SetLifeStyle(Component.For(type, impl).ImplementedBy(impl), lifeStyle));
        }
        /// <summary>
        /// 注册目标类型
        /// </summary>
        /// <typeparam name="TType">类型TType</typeparam>
        /// <param name="lifeStyle">生命周期</param>
        public void Register<TType>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton) where TType : class
        {
            IocContainer.Register(SetLifeStyle(Component.For<TType>(), lifeStyle));
        }
        /// <summary>
        /// 注册目标类型，并设置目标类型TType的具体实现类TImpl
        /// </summary>
        /// <typeparam name="TType">类型TType</typeparam>
        /// <typeparam name="TImpl">类型TImpl</typeparam>
        /// <param name="lifeStyle">生命周期</param>
        public void Register<TType, TImpl>(DependencyLifeStyle lifeStyle)
            where TType : class
            where TImpl : class, TType
        {
            IocContainer.Register(SetLifeStyle(Component.For<TType, TImpl>().ImplementedBy<TImpl>(), lifeStyle));
        }
        /// <summary>
        /// 添加约定注册接口实现类到IocManager容器
        /// </summary>
        /// <param name="reg"></param>
        public void AddConventionReg(IConventionRegister reg)
        {
            this._conventionRegster.Add(reg);
        }
        /// <summary>
        /// 为该程序集注册所有已添加
        /// </summary>
        /// <param name="assembly"></param>
        public void RegisterAssembiyByConvention(Assembly assembly, bool excuteInstaller = true)
        {
            var context = new ConventionRegsterContext(assembly, this);
            foreach (var item in _conventionRegster)
            {
                item.RegisiterAssembly(context);
            }
            if (excuteInstaller)
                IocContainer.Install(FromAssembly.Instance(assembly));
        }
        #endregion

        #region IIocResolve
        public T Resolve<T>()
        {
            return IocContainer.Resolve<T>();
        }

        public T Resolve<T>(Type type)
        {
            return (T)IocContainer.Resolve(type);
        }

        public object Resolve(Type type)
        {
            return IocContainer.Resolve(type);
        }

        #endregion

        #region IIocManager
        public bool IsRegisted(Type type)
        {
            return IocContainer.Kernel.HasComponent(type);
        }

        public bool IsRegisted<TType>()
        {
            return IsRegisted(typeof(TType));
        }
        #endregion

        #region IDispose
        public void Dispose()
        {
            IocContainer.Dispose();
        }
        #endregion

        #region 私有方法
        private ComponentRegistration<T> SetLifeStyle<T>(ComponentRegistration<T> registration, DependencyLifeStyle lifeStyle)
            where T : class
        {
            switch (lifeStyle)
            {
                case DependencyLifeStyle.Singleton:
                    return registration.LifestyleSingleton();
                case DependencyLifeStyle.Transient:
                    return registration.LifestyleTransient();
                default:
                    return registration;
            }
        }
        #endregion
    }
}
