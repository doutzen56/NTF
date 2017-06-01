using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tzen.Framework.Reflection
{
    public class TzenAssemblyFinder : IAssemblyFinder
    {
        public static TzenAssemblyFinder Instance
        {
            get
            {
                return SingletionInstance;
            }
        }
        private static readonly TzenAssemblyFinder SingletionInstance = new TzenAssemblyFinder();
        public List<Assembly> GetAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().ToList();
        }
    }
}
