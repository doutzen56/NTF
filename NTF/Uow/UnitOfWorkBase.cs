using System;

namespace NTF.Uow
{
    /// <summary>
    /// 工作单元基类，所有工作单元类继承自此类
    /// </summary>
    public abstract class UnitOfWorkBase : IUnitOfWork
    {
        /// <summary>
        /// 当前工作单元的唯一标识
        /// </summary>
        public string ID { get; private set; }
        /// <summary>
        /// 当前资源是否已释放
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// 获取当前工作单元的选项值（事务性）
        /// </summary>
        public UnitOfWorkOptions Options { get; private set; }
        /// <summary>
        /// 当前工作单元所在方法外层，是否存在工作单元
        /// </summary>
        public IUnitOfWork Outer { get; set; }
        /// <summary>
        /// 事务提交成功事件
        /// </summary>
        public event EventHandler Completed;
        /// <summary>
        /// 事务释放时事件
        /// </summary>
        public event EventHandler Disposed;
        /// <summary>
        /// 事务提交失败事件
        /// </summary>
        public event EventHandler<UnitOfWorkFailedEventArgs> Failed;
        /// <summary>
        /// 获取默认工作单元选项值
        /// </summary>
        protected IUnitOfWorkDefaultOptions DefaultOptions { get; private set; }
        /// <summary>
        /// <see cref="Begin"/>方法是否被调用过
        /// </summary>
        private bool _isBeginCalled;
        /// <summary>
        /// <see cref="Complete"/>方法是否被调用过
        /// </summary>
        private bool _isCompleteCalled;
        /// <summary>
        /// 当前工作单元是否成功完成
        /// </summary>
        private bool _succeed;
        /// <summary>
        /// 工作单元失败异常原因
        /// </summary>
        private Exception _exception;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="defaultOptions"></param>
        public UnitOfWorkBase(IUnitOfWorkDefaultOptions defaultOptions)
        {
            this.DefaultOptions = defaultOptions;
            this.ID = Guid.NewGuid().ToString("N");
        }
        /// <summary>
        /// 根据给定选项开启工作单元
        /// </summary>
        /// <param name="options">工作单元选项<see cref="UnitOfWorkOptions"/></param>
        public void Begin(UnitOfWorkOptions options)
        {
            if (options.IsNull())
            {
                throw new ArgumentNullException("options参数为null");
            }
            this.IsBeginCalled();
            this.Options = options;
            BeginUow();
        }

        public void Complete()
        {
            IsCompleteCalled();
            try
            {
                CompleteUow();
                _succeed = true;
                OnCompleted();
            }
            catch (Exception ex)
            {
                _exception = ex;
                throw;
            }
        }
        
        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            IsDisposed = true;
            if (!_succeed)
            {
                OnFailed(_exception);
            }
            DisposeUow();
            OnDisposed();
        }
        /// <summary>
        /// 提交更改
        /// </summary>
        public abstract void SaveChanges();
       
        /// <summary>
        /// 开始工作单元
        /// </summary>
        protected abstract void BeginUow();
        /// <summary>
        /// 工作单元提交
        /// </summary>
        protected abstract void CompleteUow();
        
        /// <summary>
        /// 释放资源
        /// </summary>
        protected abstract void DisposeUow();
        /// <summary>
        /// 工作单元完成事件
        /// </summary>
        protected virtual void OnCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// 提交失败事件
        /// </summary>
        /// <param name="ex">失败异常</param>
        protected virtual void OnFailed(Exception ex)
        {
            Failed?.Invoke(this, new UnitOfWorkFailedEventArgs(ex));
        }
        /// <summary>
        /// 资源释放事件
        /// </summary>
        protected virtual void OnDisposed()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// 工作单元是否已开启
        /// </summary>
        private void IsBeginCalled()
        {
            if (_isBeginCalled)
            {
                throw new Exception("请勿重复调用工作单元启动方法");
            }
            this._isBeginCalled = true;
        }
        /// <summary>
        /// 工作单元是否已提交
        /// </summary>
        private void IsCompleteCalled()
        {
            if (_isCompleteCalled)
            {
                throw new Exception("请勿重复提交");
            }
            this._isCompleteCalled = true;
        }
    }
}
