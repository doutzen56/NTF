using Castle.MicroKernel.Registration;
using NTF;
using NTF.Data;
using NTF.Data.Common;
using NTF.Data.SqlServerClient;
using NTF.Modules;
using NTF.Provider;
using NTF.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using Tzen;
using UnitTest;
using static NTF.Data.QueryProvider;

namespace NTF.控制台
{
    [DependsOn(typeof(NtfCoreModule))]
    public class TestModule : NtfModule
    {
        public override void Initialize()
        {
            base.Initialize();
            IocManager.Register<QueryLanguage, SqlLanguage>();
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

            IocManager.Register(typeof(IDbContext<>), typeof(DbQueryContenxt<>), Ioc.LifeStyle.Singleton);
        }
    }
}
