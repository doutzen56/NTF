using System;
using System.Transactions;

namespace NTF.Uow
{
    /// <summary>
    /// 工作单元选设置
    /// </summary>
    public class UnitOfWorkOptions
    {
        /// <summary>
        /// 事务作用域
        /// </summary>
        public TransactionScopeOption? Scope { get; set; }
        /// <summary>
        /// 是否事务性
        /// </summary>
        public bool? IsTransactional { get; set; }
        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan? Timeout { get; set; }
        /// <summary>
        /// 事务隔离级别
        /// </summary>
        public IsolationLevel? IsolationLevel { get; set; }
        /// <summary>
        /// 设置工作单元默认值
        /// </summary>
        internal void SetDefaultOptions(IUnitOfWorkDefaultOptions defaultOptions)
        {
            if (!Scope.HasValue)
                Scope = defaultOptions.Scope;
            if (!IsTransactional.HasValue)
                IsTransactional = defaultOptions.IsTransactional;
            if (!Timeout.HasValue)
                Timeout = defaultOptions.Timeout;
            if (!IsolationLevel.HasValue)
                IsolationLevel = defaultOptions.IsolationLevel;
        }
    }
}
