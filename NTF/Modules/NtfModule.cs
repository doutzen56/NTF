using NTF.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;

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
}
