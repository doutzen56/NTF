using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;

namespace Tzen.Framwork
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

    }
}
