using System.Reflection;

namespace NTF.Ioc
{

    /// <summary>
    /// 注册基本约定对象参数类
    /// </summary>
    public class DefaultRegsterContext
    {
        /// <summary>
        /// 要注册的程序集
        /// </summary>
        public Assembly Assembly { get; private set; }
        /// <summary>
        /// Ioc管理对象实例
        /// </summary>
        public IIocManager IocManager { get; private set; }
        public DefaultRegsterContext(Assembly assembly,IIocManager iocManager)
        {
            this.Assembly = assembly;
            this.IocManager = iocManager;
        }
    }
}
