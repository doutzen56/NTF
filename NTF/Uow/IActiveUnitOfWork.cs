using System;
using System.Threading.Tasks;

namespace NTF.Uow
{
    /// <summary>
    /// 定义当前<see cref="IUnitOfWork"/> 的基本操作
    /// </summary>
    public interface IActiveUnitOfWork
    {
        /// <summary>
        /// 事务提交成功事件
        /// </summary>
        event EventHandler Completed;
        /// <summary>
        /// 事务提交失败事件
        /// </summary>
        event EventHandler<UnitOfWorkFailedEventArgs> Failed;
        /// <summary>
        /// 事务释放时事件
        /// </summary>
        event EventHandler Disposed;
        /// <summary>
        /// 获取当前工作单元的选项值（事务性）
        /// </summary>
        UnitOfWorkOptions Options { get; }
        /// <summary>
        /// 当前资源是否已释放
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// 保存所有更改
        /// </summary>
        void SaveChanges();
        
    }
}
