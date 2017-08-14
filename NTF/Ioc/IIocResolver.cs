using System;

namespace NTF.Ioc
{
    /// <summary>
    /// 对象反转接口
    /// </summary>
    public interface IIocResolver
    {
        /// <summary>
        /// 反转指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>();
        /// <summary>
        /// 反转指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        T Resolve<T>(Type type);
        /// <summary>
        /// 反转指定类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(Type type);
        /// <summary>
        /// 判断给定类型是否被注册
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsRegistered(Type type);
        /// <summary>
        /// 判断给定类型是否被注册
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        bool IsRegistered<TType>();
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="obj"></param>
        void Release(object obj);
    }
}
