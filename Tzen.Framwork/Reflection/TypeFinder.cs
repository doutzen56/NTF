using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tzen.Framwork.Reflection
{
    public class TypeFinder : ITypeFinder
    {
        public IAssemblyFinder AssemblyFinder { get; set; }
        public TypeFinder()
        {
            AssemblyFinder = TzenAssemblyFinder.Instance;
        }
        public Type[] Find(Func<Type, bool> predicate)
        {
            return GetAllTypes().Where(predicate).ToArray();
        }

        public Type[] FindAll()
        {
            return GetAllTypes().ToArray();
        }

        private List<Type> GetAllTypes()
        {
            var allTypes = new List<Type>();
            foreach (var assembly in AssemblyFinder.GetAllAssemblies().Distinct())
            {
                try
                {
                    Type[] assemblyTypes;
                    try
                    {
                        assemblyTypes = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        assemblyTypes = ex.Types;
                    }
                    if (assemblyTypes.IsNullOrEmpty())
                    {
                        continue;
                    }
                    allTypes.AddRange(assemblyTypes.Where(a => a != null));
                }
                catch (Exception ex)
                {

                }
            }
            return allTypes;
        }
    }
}
