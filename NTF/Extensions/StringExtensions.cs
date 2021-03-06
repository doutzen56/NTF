﻿using System.Configuration;

namespace NTF
{
    public static class StringEx
    {

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 字符串格式化
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Fmt(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static string GetConnectionString(this string str)
        {
            return ConfigurationManager.ConnectionStrings[str].IsNull() ? null : ConfigurationManager.ConnectionStrings[str].ConnectionString;
        }

        public static string GetAppSettings(this string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
