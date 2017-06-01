using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Tzen.Framework.Uow
{
    /// <summary>
    /// 定义工作单元提交操作
    /// </summary>
    public interface IUnitOfWorkCompleteHandle : IDisposable
    {
        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();
        /// <summary>
        /// 提交事务
        /// </summary>
        /// <returns></returns>
        Task CommitAsync();
    }
    /// <summary>
    /// 此内部类用于处理工作单元内的事务，
    /// 实际上工作单元内的事务沿用的处于同一个工作单元下的外部事务。
    /// </summary>
    internal class DefaultUnitOfWorkCompleteHandle : IUnitOfWorkCompleteHandle
    {
        private volatile bool _isCommitCalled;
        private volatile bool _isDisposed;
        public void Commit()
        {
            _isCommitCalled = true;
        }

        public async Task CommitAsync()
        {
            _isCommitCalled = true;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            if (!_isCommitCalled)
            {
                if (HasException())
                {
                    return;
                }
                throw new Exception("工作单元执行不完整");
            }
        }
        private static bool HasException()
        {
            try
            {
                return Marshal.GetExceptionCode() != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
