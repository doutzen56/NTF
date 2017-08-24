namespace NTF.Uow
{
    /// <summary>
    /// 提供一个默认的工作单元实现
    /// </summary>
    public sealed class DefaultUnitOfWork : UnitOfWorkBase
    {
        public DefaultUnitOfWork(IUnitOfWorkDefaultOptions defaultOptions)
            : base(defaultOptions)
        {
        }

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
