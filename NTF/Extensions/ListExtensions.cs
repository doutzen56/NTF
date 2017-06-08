using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTF
{
    /// <summary>
    /// <see cref="List{T}"/>扩展方法
    /// </summary>
    public static class ListEx
    {
        #region 拓扑排序
        /// <summary>
        /// 拓扑排序，根据依赖关系来排序
        /// </summary>
        /// <remarks>
        /// 引用自：http://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">需要排序的列表</param>
        /// <param name="getDependencies"></param>
        /// <returns></returns>
        public static List<T> Sort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies)
        {
            var sorted = new List<T>();
            var visited = new Dictionary<T, bool>();

            foreach (var item in source)
            {
                Visit(item, getDependencies, sorted, visited);
            }
            return sorted;
        }

        private static void Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, List<T> sorted, Dictionary<T, bool> visited)
        {
            bool inProcess;
            var alreadyVisited = visited.TryGetValue(item, out inProcess);

            if (alreadyVisited)
            {
                if (inProcess)
                {
                    throw new ArgumentException("存在循环依赖，请检查");
                }
            }
            else
            {
                visited[item] = true;

                var dependencies = getDependencies(item);
                if (dependencies != null)
                {
                    foreach (var dependency in dependencies)
                    {
                        Visit(dependency, getDependencies, sorted, visited);
                    }
                }

                visited[item] = false;
                sorted.Add(item);
            }
        }
        #endregion

        /// <summary>
        /// 数组集合空值判断，为null或者Count=0,均返回true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || source.Count() <= 0;
        }
    }
}
