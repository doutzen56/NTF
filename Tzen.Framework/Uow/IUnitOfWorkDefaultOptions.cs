using System;
using System.Transactions;

namespace Tzen.Framework.Uow
{
    /// <summary>
    /// 设置或者获取工作单元默认值
    /// </summary>
    public interface IUnitOfWorkDefaultOptions
    {
        /// <summary>
        /// 事务作用域选项
        /// </summary>
        TransactionScopeOption Scope { get; set; }
        /// <summary>
        /// 是否工作单元事务
        /// Default:true
        /// </summary>
        bool IsTransactional { get; set; }
        /// <summary>
        /// 超时时间
        /// </summary>
        TimeSpan? Timeout { get; set; }
        IsolationLevel? IsolationLevel { get; set; }
    }

    internal class UnitOfWorkDefaultOptions : IUnitOfWorkDefaultOptions
    {
        public TransactionScopeOption Scope { get; set; }

        public bool IsTransactional { get; set; }

        public TimeSpan? Timeout { get; set; }

        public IsolationLevel? IsolationLevel { get; set; }
        public UnitOfWorkDefaultOptions()
        {
            IsTransactional = true;
            Scope = TransactionScopeOption.Required;
        }
    }
}
