using System.Reflection;

namespace Tzen.Framwork.Ioc
{

    public class ConventionRegsterContext
    {
        public Assembly Assembly { get; private set; }
        public IIocManager IocManager { get; private set; }
        public ConventionRegsterContext(Assembly assembly,IIocManager iocManager)
        {
            this.Assembly = assembly;
            this.IocManager = iocManager;
        }
    }
}
