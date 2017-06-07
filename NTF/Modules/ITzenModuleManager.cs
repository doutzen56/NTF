using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NTF.Ioc;

namespace NTF.Modules
{
    /// <summary>
    /// 定义模块管理接口
    /// </summary>
    internal interface ITzenModuleManager
    {
        void InitModules();

        void ShutdownModules();
    }
    /// <summary>
    /// 模块管理类
    /// </summary>
    internal class TzenModuleManager : ITzenModuleManager
    {
        /// <summary>
        /// 模块集合
        /// </summary>
        private readonly TzenModuleList _modules;
        private readonly IIocManager _iocManager;
        /// <summary>
        /// 模块查找器
        /// </summary>
        private readonly IModuleFinder _moduleFinder;
        public TzenModuleManager(IIocManager iocManager, IModuleFinder moduleFinder)
        {
            this._iocManager = iocManager;
            this._moduleFinder = moduleFinder;
            this._modules = new TzenModuleList();
        }
        public void InitModules()
        {
            LoadAll();
            var sortedModules = _modules.GetSortModuleListByDependency();
            sortedModules.ForEach(moduleInfo => moduleInfo.Instance.BeforeInit());
            sortedModules.ForEach(moduleInfo => moduleInfo.Instance.Initialize());
            sortedModules.ForEach(moduleInfo => moduleInfo.Instance.AfterInit());
        }

        public void ShutdownModules()
        {
            var sortedModules = _modules.GetSortModuleListByDependency();
            sortedModules.Reverse();
            sortedModules.ForEach(moduleInfo => moduleInfo.Instance.Shutdown());
        }

        private void LoadAll()
        {
            var moduleTypes = AddMissingDependedModules(_moduleFinder.FindAll());
            //模块注入
            foreach (var moduleType in moduleTypes)
            {
                if (!TzenModule.IsTzenModule(moduleType))
                {
                    throw new Exception("类型{0}不是TzenModule的子类！".Fmt(moduleType.AssemblyQualifiedName));
                }
                if (!_iocManager.IsRegistered(moduleType))
                {
                    _iocManager.Register(moduleType);
                }
            }
            //添加到模块集合
            foreach (var moduleType in moduleTypes)
            {
                var module = (TzenModule)_iocManager.Resolve(moduleType);
                module.IocManager = _iocManager;
                _modules.Add(new TzenModuleInfo(module));
            }

            //确保核心模块在首位
            var coreModuleIndex = _modules.FindIndex(a => a.Type == typeof(TzenCoreModule));
            if (coreModuleIndex > 0)
            {
                var coreModule = _modules[coreModuleIndex];
                _modules.RemoveAt(coreModuleIndex);
                _modules.Insert(0, coreModule);
            }
            SetDependencies();
        }

        /// <summary>
        /// 设置依赖项
        /// </summary>
        private void SetDependencies()
        {
            foreach (var moduleInfo in _modules)
            {
                //根据程序集引用设置
                foreach (var refAssemblyName in moduleInfo.Assembly.GetReferencedAssemblies())
                {
                    var refAssembly = Assembly.Load(refAssemblyName);
                    var dependedModuleList = _modules.Where(a => a.Assembly == refAssembly).ToList();
                    if (!dependedModuleList.IsNullOrEmpty())
                    {
                        moduleInfo.Dependencies.AddRange(dependedModuleList);
                    }
                }
                //根据模块特性DependsOnAttribute设置
                foreach (var dependedModuleType in TzenModule.FindDependedModuleTypes(moduleInfo.Type))
                {
                    var dependModuleInfo = _modules.FirstOrDefault(a => a.Type == dependedModuleType);
                    if (dependModuleInfo.IsNull())
                    {
                        throw new Exception("找不到模块{0}的依赖项{1}".Fmt(moduleInfo.Type.AssemblyQualifiedName, dependModuleInfo.Type.AssemblyQualifiedName));
                    }
                    if (moduleInfo.Dependencies.FirstOrDefault(a => a.Type == dependedModuleType).IsNull())
                    {
                        moduleInfo.Dependencies.Add(dependModuleInfo);
                    }
                }
            }
        }
        /// <summary>
        /// 添加缺失的依赖模块
        /// </summary>
        private static ICollection<Type> AddMissingDependedModules(ICollection<Type> allModules)
        {
            foreach (var item in allModules)
            {
                FillDependedModules(item, allModules);
            }
            return allModules;
        }
        private static void FillDependedModules(Type moduleType, ICollection<Type> allModules)
        {
            foreach (var item in TzenModule.FindDependedModuleTypes(moduleType))
            {
                if (!allModules.Contains(item))
                {
                    allModules.Add(item);
                    FillDependedModules(item, allModules);
                }
            }
        }
    }
}
