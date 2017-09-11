using System;

namespace NTF.Reflection
{
    /// <summary>
    /// 类型查询接口
    /// </summary>
    public interface ITypeFinder
    {
        /// <summary>
        /// 查找所有符合条件的类型
        /// </summary>
        /// <param name="predicate">查询谓语</param>
        /// <returns></returns>
        Type[] Find(Func<Type, bool> predicate);
        /// <summary>
        /// 查询所有类型
        /// </summary>
        /// <returns></returns>
        Type[] FindAll();
    }
}
