using Castle.Core;
using System.Linq;
using System.Reflection;
using Tzen.Framework.Ioc;

namespace Tzen.Framework.Uow
{
    internal class UnitOfWorkRegister
    {
        public static void Init(IIocManager iocManager)
        {
            iocManager.IocContainer.Kernel.ComponentRegistered +=
                (key, handler) =>
                {
                    if (UnitOfWorkHelper.IsUowClass(handler.ComponentModel.Implementation))
                    {
                        handler.ComponentModel.Interceptors.Add(new InterceptorReference(typeof(UnitOfWorkInterceptor)));
                    }
                    else if (handler.ComponentModel.Implementation.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(UnitOfWorkHelper.HasUowAttrbute))
                    {
                        handler.ComponentModel.Interceptors.Add(new InterceptorReference(typeof(UnitOfWorkInterceptor)));
                    }
                };
        }
    }
}
