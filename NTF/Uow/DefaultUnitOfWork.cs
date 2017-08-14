using NTF.Ioc;
using System.Data.Common;
using System.Transactions;

namespace NTF.Uow
{
    /// <summary>
    /// 提供一个默认的工作单元实现
    /// </summary>
    public sealed class DefaultUnitOfWork : UnitOfWorkBase
    {
        public DefaultUnitOfWork(IIocResolver iocResolver, IUnitOfWorkDefaultOptions defaultOptions)
            : base(defaultOptions)
        {
            this.IocResolver = iocResolver;
        }
        IIocResolver IocResolver;
        DbTransaction tran;

        public override void SaveChanges()
        {

        }


        protected override void BeginUow()
        {

        }

        protected override void CompleteUow()
        {

        }

        protected override void DisposeUow()
        {

        }
    }
}
