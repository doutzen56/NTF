namespace NTF.Modules
{
    /// <summary>
    /// 定义模块管理接口
    /// </summary>
    internal interface INtfModuleManager
    {
        /// <summary>
        /// 初始化模块
        /// </summary>
        void InitModules();
        /// <summary>
        /// 释放模块资源
        /// </summary>
        void ShutdownModules();
    }
}
