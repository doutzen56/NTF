using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tzen.Framwork.Reflection;

namespace Tzen.Framwork.Modules
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

    internal class DefaultModuleFinder : IModuleFinder
    {
        private ITypeFinder _typeFinder;
        public DefaultModuleFinder(ITypeFinder typeFinder)
        {
            this._typeFinder = typeFinder;
        }
        public ICollection<Type> FindAll()
        {
            return _typeFinder.Find(TzenModule.IsTzenModule).ToList();
        }
    }
}
