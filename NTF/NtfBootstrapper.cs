using System;
using NTF.Ioc;
using NTF.Modules;

namespace NTF
{
    /// <summary>
    /// NTF框架底层初始化类
    /// </summary>
    public class NtfBootstrapper : IDisposable
    {
        /// <summary>
        /// 基础类是否被释放
        /// </summary>
        protected bool IsDisposed;
        /// <summary>
        /// 底层Ioc管理对象
        /// </summary>
        public IIocManager IocManager { get; private set; }
        /// <summary>
        /// 底层框架模块统一管理对象
        /// </summary>
        private INtfModuleManager _moduleManager;
        /// <summary>
        /// 实例化框架基础类
        /// </summary>
        public NtfBootstrapper()
            : this(Ioc.IocManager.Instance)
        {

        }
        /// <summary>
        /// 实例化框架基础类
        /// </summary>
        /// <param name="iocManager">底层Ioc管理对象</param>
        public NtfBootstrapper(IIocManager iocManager)
        {
            this.IocManager = iocManager;
        }
        /// <summary>
        /// 初始化NTF框架基础组件
        /// </summary>
        public virtual void Init()
        {
            IocManager.IocContainer.Install(new NtfCoreInstaller());
            _moduleManager = IocManager.Resolve<INtfModuleManager>();
            _moduleManager.InitModules();
        }
        /// <summary>
        /// 释放框架所有已加载组件
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            IsDisposed = true;
            _moduleManager?.ShutdownModules();
        }
    }
}
