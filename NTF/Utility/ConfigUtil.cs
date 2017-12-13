using System.Collections.Generic;
using System.Configuration;

namespace NTF.Utility
{
    public static class ConfigUtil
    {
        public static Dictionary<string, string> GetConnStrings()
        {
            var dic = new Dictionary<string, string>();
            for (int i = 0, j = ConfigurationManager.ConnectionStrings.Count; i < j; i++)
            {
                dic.Add(
                    ConfigurationManager.ConnectionStrings[i].Name,
                    ConfigurationManager.ConnectionStrings[i].ConnectionString
                    );
            }
            return dic;
        }
    }
}
