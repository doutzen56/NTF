﻿using System.Reflection;
using NTF.Extensions;
using NTF.Ioc;
using NTF.Modules;
using NTF.Uow;

namespace NTF
{
    /// <summary>
    /// NTF核心Module
    /// </summary>
    /// <remarks>
    /// 所有使用NTF框架的Module都需要<see cref="DependsOnAttribute"/>标记
    /// </remarks>
    public sealed class TzenCoreModule : TzenModule
    {
        public override void BeforeInit()
        {
            //注册默认约定组件
            IocManager.AddDefaultRegister(new DefaultRegister());
            //工作单元注册
            UnitOfWorkRegister.Init(IocManager);
        }
        public override void Initialize()
        {
            base.Initialize();
            IocManager.RegisterAssembiyByDefault(Assembly.GetExecutingAssembly(), false);
        }
        public override void AfterInit()
        {
            base.AfterInit();
            RegisterMissingComponents();

        }
        public override void Shutdown()
        {
            base.Shutdown();
        }

        private void RegisterMissingComponents()
        {
            IocManager.RegisterIfNot<IUnitOfWork, NullUnitOfWork>(LifeStyle.Transient);
        }
    }
}