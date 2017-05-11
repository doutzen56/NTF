using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tzen.Framwork.Ioc
{
    public interface IIocManager : IIocRegister, IIocResolver, IDisposable
    {
        IWindsorContainer IocContainer { get; }
        bool IsRegisted<TType>();
        bool IsRegisted(Type type);
    }
}
