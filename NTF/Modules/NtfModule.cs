using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NTF.Ioc;

namespace NTF.Modules
{
    /// <summary>
    /// Module基类，所有模块必须继承自这个类
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public abstract class NtfModule
    {
        /// <summary>
        /// 定义一个程序集内部使用的IocManager
        /// </summary>
        protected internal IIocManager IocManager { get; internal set; }

        /// <summary>
        /// 模块初始化前
        /// </summary>
        public virtual void BeforeInit() { }
        /// <summary>
        /// 模块初始化时
        /// </summary>
        public virtual void Initialize() { }
        /// <summary>
        /// 模块初始化后
        /// </summary>
        public virtual void AfterInit() { }
        /// <summary>
        /// 应用程序终止后执行
        /// </summary>
        public virtual void Shutdown() { }
        /// <summary>
        /// 检查给定类型是否是NtfModule子类
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNtfModule(Type type)
        {
            return type.IsClass && !type.IsAbstract && typeof(NtfModule).IsAssignableFrom(type);
        }
        /// <summary>
        /// 查找指定模块中的依赖模块
        /// </summary>
        /// <param name="moduleType"></param>
        /// <returns></returns>
        public static List<Type> FindDependedModuleTypes(Type moduleType)
        {
            if (!IsNtfModule(moduleType))
            {
                throw new Exception("该类型并非派生自{0}，请检查：{1}".Fmt(typeof(NtfModule).FullName, moduleType.AssemblyQualifiedName));
            }
            var list = new List<Type>();
            if (moduleType.IsDefined(typeof(DependsOnAttribute), true))
            {
                var dependsAttrbutes = moduleType.GetCustomAttributes(typeof(DependsOnAttribute), true).Cast<DependsOnAttribute>();
                foreach (var attrbutesItem in dependsAttrbutes)
                {
                    foreach (var moduleTypeItem in attrbutesItem.DependModuleTypes)
                    {
                        list.Add(moduleTypeItem);
                    }
                }
            }
            return list;
        }
    }
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
