using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NTF.Data
{
    public interface IQueryCache<T> where T : class
    {
        ConcurrentDictionary<T, Type> Types { get; }
        ConcurrentDictionary<T, PropertyInfo[]> Fields { get; }
        ConcurrentDictionary<T, PropertyInfo[]> KeyFields { get; }
        ConcurrentDictionary<T, PropertyInfo[]> AutoIncrementFields { get; }
        ConcurrentDictionary<T, PropertyInfo[]> NoIncFields { get; }


    }
    /// <summary>
    /// 实体对象映射基类，主要用于缓存实体与数据库表字段的映射关系
    /// </summary>
    internal class QueryCache<T> where T : class
    {
        
    }
}
