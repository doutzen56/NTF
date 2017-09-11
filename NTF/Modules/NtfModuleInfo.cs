using System;
using System.Collections.Generic;
using System.Reflection;

namespace NTF.Modules
{
    /// <summary>
    /// 模块属性定义
    /// </summary>
    internal class NtfModuleInfo
    {
        /// <summary>
        /// 模块定义的程序集
        /// </summary>
        public Assembly Assembly { get; private set; }
        /// <summary>
        /// 模块类型
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// 模块实例
        /// </summary>
        public NtfModule Instance { get; private set; }
        /// <summary>
        /// 此模块相关的所有模块
        /// </summary>
        public List<NtfModuleInfo> Dependencies { get; private set; }
        /// <summary>
        /// 创建一个新的NtfModuleInfo对象
        /// </summary>
        /// <param name="instance"></param>
        public NtfModuleInfo(NtfModule instance)
        {
            Dependencies = new List<NtfModuleInfo>();
            Type = instance.GetType();
            Instance = instance;
            Assembly = Type.Assembly;
        }
    }
}
