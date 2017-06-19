using Newtonsoft.Json;

namespace NTF
{
    public static class ObjectEx
    {
        public static bool IsNull<T>(this T obj) where T : class
        {
            return obj == null;
        }

        public static string ToJson(this object obj, bool ignoreNull = false, string dateFormatString = "yyyy-MM-dd HH:mm:ss")
        {
            return JsonConvert.SerializeObject(
                obj,
                Formatting.None,
                new JsonSerializerSettings
                {
                    DateFormatString = dateFormatString,
                    NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
                });
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
