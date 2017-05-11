using System.Reflection;
using Tzen.Framwork.Ioc;
using Tzen.Framwork.Modules;

namespace Tzen.Framwork
{
    /// <summary>
    /// Tzen.Framework核心Module
    /// </summary>
    /// <remarks>
    /// 所有使用Tzen.Framwork框架的Module都需要<see cref="DependsOnAttribute"/>标记
    /// </remarks>
    public sealed class TzenCoreModule : TzenModule
    {
        public override void BeforeInit()
        {
            IocManager.AddConventionReg(new BasicConventionRegister());
        }
        public override void Initialize()
        {
            base.Initialize();
            IocManager.RegisterAssembiyByConvention(Assembly.GetExecutingAssembly(),false);
        }
        public override void AfterInit()
        {
            base.AfterInit();
        }
        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}
