using System;
using System.Collections.Generic;

namespace NTF.Modules
{
    /// <summary>
    /// 负责查找所有模块的接口定义
    /// </summary>
    public interface IModuleFinder
    {
        /// <summary>
        /// 查找所有模块
        /// </summary>
        /// <returns></returns>
        ICollection<Type> FindAll();
    }
}
