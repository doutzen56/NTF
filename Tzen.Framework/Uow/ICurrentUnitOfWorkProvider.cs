namespace Tzen.Framework.Uow
{
    /// <summary>
    /// 此接口用于获取或设置当前 <see cref="IUnitOfWork"/>
    /// </summary>
    public interface ICurrentUnitOfWorkProvider
    {
        /// <summary>
        /// 获取或设置当前 <see cref="IUnitOfWork"/>
        /// </summary>
        IUnitOfWork Current { get; set; }
    }
}
