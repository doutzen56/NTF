using Castle.DynamicProxy;
using Tzen.Framework.Aop;

namespace Tzen.Framework.Uow
{
    internal class UnitOfWorkInterceptor : InterceptorBase
    {
        private readonly IUnitOfWorkManager _uowManager;
        public UnitOfWorkInterceptor(IUnitOfWorkManager uowManager)
        {
            _uowManager = uowManager;
        }
        public override void Intercept(IInvocation invocation)
        {
            if (_uowManager.Current == null)
            {
                invocation.Proceed();
                return;
            }
            var uowAttr = UnitOfWorkHelper.GetUnitOfWorkAttributeOrNull(invocation.MethodInvocationTarget);
            if (uowAttr == null || uowAttr.IsDisabled)
            {
                invocation.Proceed();
                return;
            }
            using(var uow = _uowManager.Begin(uowAttr.CreateOptions()))
            {
                invocation.Proceed();
                uow.Commit();
            }
        }
    }
}
