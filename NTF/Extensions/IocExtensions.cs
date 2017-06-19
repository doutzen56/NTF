using System;
using NTF.Ioc;

namespace NTF
{
    public static class IocExtensions
    {
        /// <summary>
        /// 注册没有被注册过的目标类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iocRegister"></param>
        /// <param name="lifeStyle"></param>
        /// <returns>没有被注册过，则返回 true</returns>
        public static bool RegisterIfNot<T>(this IIocRegister iocRegister, LifeStyle lifeStyle = LifeStyle.Singleton)
            where T : class
        {
            if (iocRegister.IsRegistered<T>())
            {
                return false;
            }

            iocRegister.Register<T>(lifeStyle);
            return true;
        }
        /// <summary>
        /// 注册没有被注册过的目标类型
        /// </summary>
        /// <param name="iocRegister"></param>
        /// <param name="type"></param>
        /// <param name="lifeStyle"></param>
        /// <returns>没有被注册过，则返回 true</returns>
        public static bool RegisterIfNot(this IIocRegister iocRegister, Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            if (iocRegister.IsRegistered(type))
            {
                return false;
            }

            iocRegister.Register(type, lifeStyle);
            return true;
        }
        /// <summary>
        /// 注册没有被注册过的目标类型
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="iocRegister"></param>
        /// <param name="lifeStyle"></param>
        /// <returns>没有被注册过，则返回 true</returns>
        public static bool RegisterIfNot<TType, TImpl>(this IIocRegister iocRegister, LifeStyle lifeStyle = LifeStyle.Singleton)
            where TType : class
            where TImpl : class, TType
        {
            if (iocRegister.IsRegistered<TType>())
            {
                return false;
            }

            iocRegister.Register<TType, TImpl>(lifeStyle);
            return true;
        }
        /// <summary>
        /// 注册没有被注册过的目标类型
        /// </summary>
        /// <param name="iocRegister"></param>
        /// <param name="type"></param>
        /// <param name="impl"></param>
        /// <param name="lifeStyle"></param>
        /// <returns>没有被注册过，则返回 true</returns>
        public static bool RegisterIfNot(this IIocRegister iocRegister, Type type, Type impl, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            if (iocRegister.IsRegistered(type))
            {
                return false;
            }

            iocRegister.Register(type, impl, lifeStyle);
            return true;
        }
    }
}
