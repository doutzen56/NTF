namespace NTF.Uow
{
    /// <summary>
    /// 定义一个工作单元，
    /// 这个接口是框架内部使用。
    /// 应用程序使用<see cref="Begin(UnitOfWorkOptions)"/>来开启一个新的事务
    /// </summary>
    public interface IUnitOfWork : IActiveUnitOfWork, IUnitOfWorkCompleteHandle
    {
        /// <summary>
        /// 当前工作单元的唯一标识
        /// </summary>
        string ID { get; }
        /// <summary>
        /// 当前工作单元所在方法外层，是否存在工作单元
        /// </summary>
        IUnitOfWork Outer { get; set; }
        /// <summary>
        /// 根据给定选项开启工作单元
        /// </summary>
        /// <param name="options">工作单元选项<see cref="UnitOfWorkOptions"/></param>
        void Begin(UnitOfWorkOptions options);
    }
}
