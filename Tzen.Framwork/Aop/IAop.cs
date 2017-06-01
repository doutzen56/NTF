using Castle.DynamicProxy;

namespace Tzen.Framework.Aop
{
    public interface IAop: IInterceptor
    {
        /// <summary>
        /// 方法执行前操作
        /// </summary>
        /// <param name="callContext"></param>
        void PerformBefore(IInvocation callContext);
        /// <summary>
        /// 方法执行后操作
        /// </summary>
        /// <param name="callContext"></param>
        void PerformAfter(IInvocation callContext);
    }
}
