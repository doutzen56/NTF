using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NTF.Reflection
{
    public class NtfAssemblyFinder : IAssemblyFinder
    {
        public static NtfAssemblyFinder Instance
        {
            get
            {
                return SingletionInstance;
            }
        }
        private static readonly NtfAssemblyFinder SingletionInstance = new NtfAssemblyFinder();
        public List<Assembly> GetAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().ToList();
        }
    }
}
