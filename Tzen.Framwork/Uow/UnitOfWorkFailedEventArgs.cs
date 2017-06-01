using System;

namespace Tzen.Framework.Uow
{
    /// <summary>
    /// <see cref="IActiveUnitOfWork.Failed"/>事件的事件参数
    /// </summary>
    public class UnitOfWorkFailedEventArgs : EventArgs
    {
        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; private set; }
        public UnitOfWorkFailedEventArgs(Exception ex)
        {
            this.Exception = ex;
        }
    }
}
