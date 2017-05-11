using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tzen.Framwork.Ioc;
using Tzen.Framwork.Modules;

namespace Tzen.Framwork
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
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
