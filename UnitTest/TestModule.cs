using Castle.MicroKernel.Registration;
using NTF.Data;
using NTF.Data.Common;
using NTF.Data.SqlServerClient;
using NTF.Modules;
using NTF.Provider;
using NTF.Provider.Data;
using NTF.Utility;
using System;
using System.Reflection;

namespace NTF.控制台
{
    [DependsOn(typeof(NtfCoreModule))]
    public class TestModule : NtfModule
    {
        public override void Initialize()
        {
            base.Initialize();
            IocManager.Register<QueryLanguage, SqlLanguage>();
            IocManager.RegisterAssembiyByDefault(Assembly.GetExecutingAssembly(), false);
        }
        public override void AfterInit()
        {
            RegisterDbContext();
            base.AfterInit();

        }

        private void RegisterDbContext()
        {

            foreach (var item in ConfigUtil.GetConnStrings())
            {
                    IocManager.IocContainer.Register(
                        Component.For<Func<string, QueryProvider>>().UsingFactoryMethod(
                           p =>
                               {
                                   return (Func<string, QueryProvider>)(named => DbQueryProvider.From(named));
                               })
                               .Named(item.Key));
            }

            IocManager.Register(typeof(IDbContext<>), typeof(DbContenxt<>), Ioc.LifeStyle.Singleton);
        }
    }
}
