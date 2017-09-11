using System;
using System.Collections.Generic;
using System.Linq;

namespace NTF.Modules
{
    /// <summary>
    /// 用于存储<see cref="NtfModuleInfo"/>的字典类
    /// </summary>
    internal class NtfModuleList : List<NtfModuleInfo>
    {
        /// <summary>
        /// 获取一个模块的引用实例
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <returns></returns>
        public TModule GetModule<TModule>() where TModule : NtfModule
        {
            var module = this.FirstOrDefault(a => a.Type == typeof(TModule));
            if (module.IsNull())
            {
                throw new Exception("未找到模块：{0}".Fmt(typeof(TModule).FullName));
            }
            return (TModule)module.Instance;
        }
        /// <summary>
        /// 根据依赖关系对模块进行排序。
        /// 如：模块A,依赖于模块B，那么返回结果：B，A
        /// </summary>
        /// <returns>返回根据依赖关系排序后的列表</returns>
        public List<NtfModuleInfo> GetSortModuleListByDependency()
        {
            var sortedModules = ListEx.Sort(this, a => a.Dependencies);
            //确保核心模块在首位
            var coreModuleIndex = sortedModules.FindIndex(a => a.Type == typeof(NtfCoreModule));
            if (coreModuleIndex > 0)
            {
                var coreModule = sortedModules[coreModuleIndex];
                sortedModules.RemoveAt(coreModuleIndex);
                sortedModules.Insert(0, coreModule);
            }
            return sortedModules;
        }
    }
}
