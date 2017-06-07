using System;
using NTF.Ioc;
using NTF.Modules;

namespace NTF
{
    public class TzenBootstrapper : IDisposable
    {
        protected bool IsDisposed;
        public IIocManager IocManager { get; private set; }

        private ITzenModuleManager _moduleManager;
        public TzenBootstrapper()
            : this(Ioc.IocManager.Instance)
        {

        }
        public TzenBootstrapper(IIocManager iocManager)
        {
            this.IocManager = iocManager;
        }
        public virtual void Init()
        {
            IocManager.IocContainer.Install(new TzenCoreInstaller());
            _moduleManager = IocManager.Resolve<ITzenModuleManager>();
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
