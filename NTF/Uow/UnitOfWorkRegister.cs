using Castle.Core;
using System.Linq;
using System.Reflection;
using NTF.Ioc;

namespace NTF.Uow
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
