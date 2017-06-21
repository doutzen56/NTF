using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NTF.Reflection
{
    public class TypeFinder : ITypeFinder
    {
        public IAssemblyFinder AssemblyFinder { get; set; }
        public TypeFinder()
        {
            AssemblyFinder = NtfAssemblyFinder.Instance;
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
                catch
                {

                }
            }
            var type = allTypes.Where(a => a.Name == "DataModule" || a.Name.IndexOf("Module") > 0).ToList();
            return allTypes;
        }
    }
}
