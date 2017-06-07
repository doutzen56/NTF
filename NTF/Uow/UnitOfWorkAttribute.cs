using System;
using System.Transactions;

namespace NTF.Uow
{
    /// <summary>
    /// 标记方法是否使用事务提交，如果标记启用事务提交，则所有操作将在打开数据库后一并提交，失败将回滚
    /// </summary>
    /// <remarks>
    /// 如果调用此方法之外已存在一个工作单元，并不会影响，因为他们将会使用同一个事务提交
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class UnitOfWorkAttribute : Attribute
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
        /// 设置是否禁用工作单元
        /// </summary>
        public bool IsDisabled { get; set; }
        /// <summary>
        /// 创建<see cref="UnitOfWorkAttribute"/>新实例
        /// </summary>
        public UnitOfWorkAttribute()
        {

        }
        /// <summary>
        /// 创建<see cref="UnitOfWorkAttribute"/>新实例
        /// </summary>
        /// <param name="isTransactional">是否事务化</param>
        public UnitOfWorkAttribute(bool isTransactional)
        {
            this.IsTransactional = isTransactional;
        }
        /// <summary>
        /// 创建<see cref="UnitOfWorkAttribute"/>新实例
        /// </summary>
        /// <param name="timeOut">超时时间，单位（秒）</param>
        public UnitOfWorkAttribute(int timeOut)
        {
            this.Timeout = TimeSpan.FromSeconds(timeOut);
        }
        /// <summary>
        /// 创建<see cref="UnitOfWorkAttribute"/>新实例
        /// </summary>
        /// <param name="isTransactional">是否事务化</param>
        /// <param name="timeout">超时时间，单位（秒）</param>
        public UnitOfWorkAttribute(bool isTransactional, int timeout)
        {
            IsTransactional = isTransactional;
            Timeout = TimeSpan.FromSeconds(timeout);
        }
        /// <summary>
        /// 创建<see cref="UnitOfWorkAttribute"/>新实例
        /// </summary>
        /// <param name="isolationLevel">事务隔离级别</param>
        public UnitOfWorkAttribute(IsolationLevel isolationLevel)
        {
            IsTransactional = true;
            IsolationLevel = isolationLevel;
        }
        /// <summary>
        /// 创建<see cref="UnitOfWorkAttribute"/>新实例
        /// </summary>
        /// <param name="isolationLevel">事务隔离级别</param>
        /// <param name="timeout">超时时间，单位（秒）</param>
        public UnitOfWorkAttribute(IsolationLevel isolationLevel, int timeout)
        {
            IsTransactional = true;
            IsolationLevel = isolationLevel;
            Timeout = TimeSpan.FromSeconds(timeout);
        }
        /// <summary>
        /// 创建<see cref="UnitOfWorkAttribute"/>新实例
        /// </summary>
        /// <param name="scope">事务作用域</param>
        public UnitOfWorkAttribute(TransactionScopeOption scope)
        {
            IsTransactional = true;
            Scope = scope;
        }
        /// <summary>
        /// 创建<see cref="UnitOfWorkAttribute"/>新实例
        /// </summary>
        /// <param name="scope">事务作用域</param>
        /// <param name="timeout">超时时间，单位（秒）</param>
        public UnitOfWorkAttribute(TransactionScopeOption scope, int timeout)
        {
            IsTransactional = true;
            Scope = scope;
            Timeout = TimeSpan.FromSeconds(timeout);
        }

        internal UnitOfWorkOptions CreateOptions()
        {
            return new UnitOfWorkOptions
            {
                IsTransactional = IsTransactional,
                IsolationLevel = IsolationLevel,
                Timeout = Timeout,
                Scope = Scope
            };
        }
    }
}
