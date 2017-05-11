using System;

namespace Tzen.Framwork.Modules
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependsOnAttribute : Attribute
    {
        /// <summary>
        /// 用于定义Tzen.Framework与其他模块的依赖关系
        /// </summary>
        /// <param name="dependModuleTypes"></param>
        public DependsOnAttribute(params Type[] dependModuleTypes)
        {
            this.DependModuleTypes = dependModuleTypes;
        }
        /// <summary>
        /// 依赖的模块类型
        /// </summary>
        public Type[] DependModuleTypes { get; private set; }
    }
}
