﻿namespace NTF.Ioc
{
    /// <summary>
    /// 定义注入对象生命周期：临时/单例
    /// </summary>
    public enum LifeStyle
    {
        /// <summary>
        /// 单例对象。第一次创建后，后续将使用相同对象
        /// </summary>
        Singleton,
        /// <summary>
        /// 临时对象。每次Resolve都会创建一个新的对象出来
        /// </summary>
        Transient
    }
}
