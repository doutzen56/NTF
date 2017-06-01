using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Tzen.Framework.Extensions;

namespace Tzen.Framework.Aop
{
    /// <summary>
    /// 拦截器基类，所有拦截器都要继承自此类
    /// </summary>
    public abstract class InterceptorBase : IAop
    {
        public virtual void Intercept(IInvocation invocation)
        {
            PerformBefore(invocation);
            invocation.Proceed();
            PerformAfter(invocation);
        }
        public virtual void PerformAfter(IInvocation callContext)
        {
        }
        public virtual void PerformBefore(IInvocation callContext)
        {
        }
    }
}
