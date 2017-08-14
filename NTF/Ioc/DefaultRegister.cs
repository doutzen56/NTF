using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;

namespace NTF.Ioc
{
    /// <summary>
    /// 注册基本约定实现，如<see cref="ITransient"/>、<see cref="ISingleton"/>和<see cref="IInterceptor"/>
    /// </summary>
    public class DefaultRegister : IDefaultRegister
    {
        /// <summary>
        /// 注册基本约定对象，如<see cref="ITransient"/>、<see cref="ISingleton"/>和<see cref="IInterceptor"/>
        /// </summary>
        /// <param name="context"></param>
        public void RegisiterAssembly(DefaultRegsterContext context)
        {
            //注入ITransient接口对象
            context.IocManager.IocContainer.Register(
                Classes.FromAssembly(context.Assembly)
                    .IncludeNonPublicTypes()
                    .BasedOn<ITransient>()
                    .WithService.Self()
                    .WithService.DefaultInterfaces()
                    .LifestyleTransient()
                );
            //注入ISingleton接口对象
            context.IocManager.IocContainer.Register(
                Classes.FromAssembly(context.Assembly)
                    .IncludeNonPublicTypes()
                    .BasedOn<ISingleton>()
                    .WithService.Self()
                    .WithService.DefaultInterfaces()
                    .LifestyleSingleton()
                );
            //注入IInterceptor接口对象
            context.IocManager.IocContainer.Register(
                Classes.FromAssembly(context.Assembly)
                    .IncludeNonPublicTypes()
                    .BasedOn<IInterceptor>()
                    .WithService.Self()
                    .LifestyleTransient()
                );
        }
    }
}
