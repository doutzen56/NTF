using System;
using NTF.Ioc;
using NTF.Modules;

namespace NTF
{
    public class NtfBootstrapper : IDisposable
    {
        protected bool IsDisposed;
        public IIocManager IocManager { get; private set; }

        private INtfModuleManager _moduleManager;
        public NtfBootstrapper()
            : this(Ioc.IocManager.Instance)
        {

        }
        public NtfBootstrapper(IIocManager iocManager)
        {
            this.IocManager = iocManager;
        }
        public virtual void Init()
        {
            IocManager.IocContainer.Install(new NtfCoreInstaller());
            _moduleManager = IocManager.Resolve<INtfModuleManager>();
            _moduleManager.InitModules();
        }
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
