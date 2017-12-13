using System;
using System.Runtime.InteropServices;

namespace NTF.Uow
{
    /// <summary>
    /// 定义工作单元提交操作
    /// </summary>
    public interface IUnitOfWorkCompleteHandle : IDisposable
    {
        /// <summary>
        /// 完成工作单元，
        /// 保存所有更改并提交事务
        /// </summary>
        void Complete();
    }
    /// <summary>
    /// 此内部类用于处理工作单元内的事务，
    /// 实际上工作单元内的事务沿用的处于同一个工作单元下的外部事务。
    /// </summary>
    internal class DefaultUnitOfWorkCompleteHandle : IUnitOfWorkCompleteHandle
    {
        private volatile bool _isCompleteCalled;
        private volatile bool _isDisposed;
        public void Complete()
        {
            _isCompleteCalled = true;
        }
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            if (!_isCompleteCalled)
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
