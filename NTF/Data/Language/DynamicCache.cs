using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTF.Data.Language
{
    public static class DynamicCache
    {
        private static Dictionary<RuntimeTypeHandle, string> insertSQL;
        private static Dictionary<RuntimeTypeHandle, string> updateSQL;
        private static Dictionary<RuntimeTypeHandle, string> deleteSQL;
        static DynamicCache()
        {
            insertSQL = new Dictionary<RuntimeTypeHandle, string>();
            updateSQL = new Dictionary<RuntimeTypeHandle, string>();
            deleteSQL = new Dictionary<RuntimeTypeHandle, string>();
        }
        public static string GetInserSql(Type type)
        {
            if (insertSQL[type.TypeHandle].IsNullOrEmpty())
            {

            }
            return null;
        }
    }
}
