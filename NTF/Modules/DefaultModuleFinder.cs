using NTF.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTF.Modules
{

    internal class DefaultModuleFinder : IModuleFinder
    {
        private ITypeFinder _typeFinder;
        public DefaultModuleFinder(ITypeFinder typeFinder)
        {
            this._typeFinder = typeFinder;
        }
        public ICollection<Type> FindAll()
        {
            return _typeFinder.Find(NtfModule.IsNtfModule).ToList();
        }
    }
}
