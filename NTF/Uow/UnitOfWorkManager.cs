﻿using System.Transactions;
using NTF.Ioc;

namespace NTF.Uow
{
    /// <summary>
    /// 工作单元管理类
    /// </summary>
    internal class UnitOfWorkManager : IUnitOfWorkManager, ITransient
    {
        private readonly IIocResolver _iocResolver;
        private readonly ICurrentUnitOfWorkProvider _currentUowProvider;
        private readonly IUnitOfWorkDefaultOptions _defaultOptions;
        public UnitOfWorkManager(
            IIocResolver iocResolver,
            ICurrentUnitOfWorkProvider currentUowProvider,
            IUnitOfWorkDefaultOptions defaultOptions)
        {
            this._iocResolver = iocResolver;
            this._currentUowProvider = currentUowProvider;
            this._defaultOptions = defaultOptions;
        }
        public IActiveUnitOfWork Current
        {
            get
            {
                return _currentUowProvider.Current;
            }
        }

        public IUnitOfWorkCompleteHandle Begin()
        {
            return Begin(new UnitOfWorkOptions());
        }

        public IUnitOfWorkCompleteHandle Begin(TransactionScopeOption scope)
        {
            return Begin(new UnitOfWorkOptions() { Scope = scope });
        }

        public IUnitOfWorkCompleteHandle Begin(UnitOfWorkOptions options)
        {
            options.SetDefaultOptions(_defaultOptions);
            if (options.Scope == TransactionScopeOption.Required && _currentUowProvider.Current != null)
            {
                return new DefaultUnitOfWorkCompleteHandle();
            }
            var uow = _iocResolver.Resolve<IUnitOfWork>();
            uow.Completed += (sender, args) =>
            {
                _currentUowProvider.Current = null;
            };
            uow.Failed += (sender, args) =>
            {
                _currentUowProvider.Current = null;
            };
            uow.Disposed += (sender, args) =>
            {
                _iocResolver.Release(uow);
            };
            uow.Begin(options);
            _currentUowProvider.Current = null;
            return uow;
        }
    }
}
