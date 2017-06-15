using System;
using System.Reflection;

namespace NTF.Provider
{
    /// <summary>
    /// 将强类型委托转换为弱类型方法
    /// </summary>
    public class StrongDelegate
    {
        Func<object[], object> fn;

        private StrongDelegate(Func<object[], object> fn)
        {
            this.fn = fn;
        }

        private static MethodInfo[] _meths;

        static StrongDelegate()
        {
            _meths = new MethodInfo[9];

            var meths = typeof(StrongDelegate).GetMethods();
            for (int i = 0, n = meths.Length; i < n; i++)
            {
                var gm = meths[i];
                if (gm.Name.StartsWith("M"))
                {
                    var tas = gm.GetGenericArguments();
                    _meths[tas.Length - 1] = gm;
                }
            }
        }

        /// <summary>
        /// 基于弱类型委托创建强类型委托
        /// </summary>
        /// <param name="delegateType">强类型委托的类型</param>
        /// <param name="target">弱类型参数</param>
        /// <param name="method">获取单个对象数组并返回对象的任何方法</param>
        /// <returns></returns>
        public static Delegate CreateDelegate(Type delegateType, object target, MethodInfo method)
        {
            return CreateDelegate(delegateType, (Func<object[], object>)Delegate.CreateDelegate(typeof(Func<object[], object>), target, method));
        }

        /// <summary>
        /// 基于弱类型委托创建强类型委托
        /// </summary>
        /// <param name="delegateType">强类型委托的类型</param>
        /// <param name="fn">弱类型委托</param>
        /// <returns></returns>
        public static Delegate CreateDelegate(Type delegateType, Func<object[], object> fn)
        {
            MethodInfo invoke = delegateType.GetMethod("Invoke");
            var parameters = invoke.GetParameters();
            Type[] typeArgs = new Type[1 + parameters.Length];
            for (int i = 0, n = parameters.Length; i < n; i++)
            {
                typeArgs[i] = parameters[i].ParameterType;
            }
            typeArgs[typeArgs.Length - 1] = invoke.ReturnType;
            if (typeArgs.Length <= _meths.Length)
            {
                var gm = _meths[typeArgs.Length - 1];
                var m = gm.MakeGenericMethod(typeArgs);
                return Delegate.CreateDelegate(delegateType, new StrongDelegate(fn), m);
            }
            throw new NotSupportedException("委托参数太多");
        }

        public TResult M<TResult>()
        {
            return (TResult)fn(null);
        }

        public TResult M<T1, TResult>(T1 t1)
        {
            return (TResult)fn(new object[] { t1 });
        }

        public TResult M<T1, T2, TResult>(T1 a1, T2 a2)
        {
            return (TResult)fn(new object[] { a1, a2 });
        }

        public TResult M<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 a3)
        {
            return (TResult)fn(new object[] { t1, t2, a3 });
        }

        public TRsult M<T1, T2, T3, T4, TRsult>(T1 a1, T2 a2, T3 a3, T4 a4)
        {
            return (TRsult)fn(new object[] { a1, a2, a3, a4 });
        }

        public TRsult M<T1, T2, T3, T4, T5, TRsult>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            return (TRsult)fn(new object[] { a1, a2, a3, a4, a5 });
        }

        public TRsult M<T1, T2, T3, T4, T5, T6, TRsult>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            return (TRsult)fn(new object[] { a1, a2, a3, a4, a5, a6 });
        }

        public TRsult M<T1, T2, T3, T4, T5, T6, T7, TRsult>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            return (TRsult)fn(new object[] { a1, a2, a3, a4, a5, a6, a7 });
        }

        public TRsult M<T1, T2, T3, T4, T5, T6, T7, T8, TRsult>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            return (TRsult)fn(new object[] { a1, a2, a3, a4, a5, a6, a7, a8 });
        }
    }
}