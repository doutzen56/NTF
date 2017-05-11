using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tzen.Framwork.Ioc
{
    /// <summary>
    /// 定义注入对象类型：瞬态/单例
    /// </summary>
    public enum DependencyLifeStyle
    {
        /// <summary>
        /// 单例对象。第一次创建后，后续将使用相同对象
        /// </summary>
        Singleton,
        /// <summary>
        /// 瞬态对象。每次Resolve都会创建一个新的对象出来
        /// </summary>
        Transient
    }
}
