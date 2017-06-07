using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NTF.Reflection;
using NTF.Modules;
using NTF.Uow;

namespace NTF.Ioc
{
    /// <summary>
    /// 核心初始化注入
    /// </summary>
    /// <remarks>
    /// 框架底层组件注入
    /// </remarks>
    public class TzenCoreInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                    Component.For<IUnitOfWorkDefaultOptions, UnitOfWorkDefaultOptions>().ImplementedBy<UnitOfWorkDefaultOptions>().LifestyleSingleton(),
                    Component.For<ITypeFinder>().ImplementedBy<TypeFinder>().LifestyleSingleton(),
                    Component.For<IModuleFinder>().ImplementedBy<DefaultModuleFinder>().LifestyleTransient(),
                    Component.For<ITzenModuleManager>().ImplementedBy<TzenModuleManager>().LifestyleSingleton()
                );
        }
    }
}
