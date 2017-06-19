using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NTF
{
    public static class TaskEx
    {
        /// <summary>
        /// 检查当前方法是否异步方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsAsyncMethod(MethodInfo method)
        {
            return (
                method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                );
        }

        /// <summary>
        /// 处理异步方法执行过程
        /// </summary>
        /// <param name="actualReturnValue">实际返回值</param>
        /// <param name="finalAction">finally逻辑块</param>
        /// <returns></returns>
        public static async Task AwaitTaskWithFinally(Task actualReturnValue, Action<Exception> finalAction)
        {
            Exception exception = null;
            try
            {
                await actualReturnValue;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                finalAction?.Invoke(exception);
            }
        }
        /// <summary>
        /// 处理异步方法执行
        /// </summary>
        /// <param name="actualReturnValue">实际返回值</param>
        /// <param name="postAction">逻辑块</param>
        /// <param name="finalAction">finally逻辑块</param>
        /// <returns></returns>
        public static async Task AwaitTaskWithPostActionAndFinally(Task actualReturnValue, Func<Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;
            try
            {
                await actualReturnValue;
                await postAction();
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                finalAction?.Invoke(exception);
            }
        }

        public static async Task<T> AwaitTaskWithFinallyAndGetResult<T>(Task<T> actualReturnValue, Action<Exception> finalAction)
        {
            Exception exception = null;

            try
            {
                return await actualReturnValue;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                finalAction(exception);
            }
        }
    }
}
