using System.Threading.Tasks;

namespace NTF.Uow
{
    /// <summary>
    /// 提供一个空的工作单元实现
    /// </summary>
    /// <remarks>
    /// 用于没有数据库情况下使用工作单元
    /// </remarks>
    public sealed class NullUnitOfWork : UnitOfWorkBase
    {
        public NullUnitOfWork(IUnitOfWorkDefaultOptions defaultOptions)
            : base(defaultOptions)
        {
        }

        public override void SaveChanges()
        {

        }

        public override async Task SaveChangesAsync()
        {

        }

        protected override void BeginUow()
        {

        }

        protected override void CommitUow()
        {

        }

        protected override async Task CommitUowAsync()
        {

        }

        protected override void DisposeUow()
        {

        }
    }
}
