namespace Tzen.Framwork.SQL
{
    public static class DbLock
    {
        /// <summary>
        /// 默认设置
        /// </summary>
        public const string Default = "";
        /// <summary>
        /// 允许脏读数据
        /// </summary>
        public const string NoLock = "(NOLOCK)";
    }
}
