using Castle.MicroKernel.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Tzen.Framwork.Reflection;
using Tzen.Framwork.Modules;

namespace Tzen.Framwork.Ioc
{
    /// <summary>
    /// 核心初始化注入
    /// </summary>
    public class TzenCoreInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                    Component.For<ITypeFinder>().ImplementedBy<TypeFinder>().LifestyleSingleton(),
                    Component.For<IModuleFinder>().ImplementedBy<DefaultModuleFinder>().LifestyleTransient(),
                    Component.For<ITzenModuleManager>().ImplementedBy<TzenModuleManager>().LifestyleSingleton()
                );
        }
    }
}
