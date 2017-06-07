using System.Transactions;

namespace NTF.Uow
{
    /// <summary>
    /// 工作单元管理接口。
    /// 用于开启或控制一个工作单元
    /// </summary>
    public interface IUnitOfWorkManager
    {
        /// <summary>
        /// 获取当前工作单元
        /// </summary>
        IActiveUnitOfWork Current { get; }
        /// <summary>
        /// 开启一个新的事务
        /// </summary>
        /// <returns>返回一个可操作的工作单元Handle</returns>
        IUnitOfWorkCompleteHandle Begin();
        /// <summary>
        /// 开启一个新的事务
        /// </summary>
        /// <param name="scope"><see cref="TransactionScopeOption"/></param>
        /// <returns>返回一个可操作的工作单元Handle</returns>
        IUnitOfWorkCompleteHandle Begin(TransactionScopeOption scope);
        /// <summary>
        /// 开启一个新的事务
        /// </summary>
        /// <param name="options"><see cref="UnitOfWorkOptions"/></param>
        /// <returns>返回一个可操作的工作单元Handle</returns>
        IUnitOfWorkCompleteHandle Begin(UnitOfWorkOptions options);
    }
}
