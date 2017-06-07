using Castle.Core;
using System;
using System.Collections.Concurrent;
using System.Runtime.Remoting.Messaging;
using NTF.Ioc;

namespace NTF.Uow
{
    /// <summary>
    /// 获取当前工作单元默认实现
    /// </summary>
    public class DefaultCurrentUnitOfWorkProvider : ICurrentUnitOfWorkProvider, ITransient
    {
        /// <summary>
        /// 当前工作单元
        /// </summary>
        /// <remarks>
        /// 此属性不会被Ioc注入
        /// </remarks>
        [DoNotWire]
        public IUnitOfWork Current
        {
            get
            {
                return GetCurrentUow();
            }

            set
            {
                SetCurrentUow(value);
            }
        }

        private const string ContextKey = "NTF.CurrentUow";
        private static readonly ConcurrentDictionary<string, IUnitOfWork> UnitOfWorkDictionary = new ConcurrentDictionary<string, IUnitOfWork>();
        public DefaultCurrentUnitOfWorkProvider()
        {
        }
        private static IUnitOfWork GetCurrentUow()
        {
            //根据给定key在上下文中查找工作单元
            var unitOfWorkKey = CallContext.LogicalGetData(ContextKey) as string;
            //没找到返回null
            if (unitOfWorkKey.IsNull())
                return null;
            IUnitOfWork unitOfWork;
            //存储字典中是否存在key为默认值的工作单元
            //不存在则返回
            if (!UnitOfWorkDictionary.TryGetValue(unitOfWorkKey, out unitOfWork))
            {
                CallContext.FreeNamedDataSlot(ContextKey);
                return null;
            }
            if (unitOfWork.IsDisposed)
            {
                UnitOfWorkDictionary.TryRemove(unitOfWorkKey, out unitOfWork);
                CallContext.FreeNamedDataSlot(ContextKey);
                return null;
            }
            return unitOfWork;
        }
        private static void SetCurrentUow(IUnitOfWork value)
        {
            if (value.IsNull())
            {
                ExitCurrentUow();
                return;
            }
            var unitOfWorkKey = CallContext.LogicalGetData(ContextKey) as string;
            if (!unitOfWorkKey.IsNull())
            {
                IUnitOfWork outer;
                if (UnitOfWorkDictionary.TryGetValue(unitOfWorkKey, out outer))
                {
                    if (outer != value)
                    {
                        value.Outer = outer;
                    }
                }
            }
            unitOfWorkKey = value.ID;
            if (!UnitOfWorkDictionary.TryAdd(unitOfWorkKey, value))
            {
                throw new Exception("内部异常：添加工作单元到字典发生了不可预知的错误");
            }
            CallContext.LogicalSetData(ContextKey, unitOfWorkKey);
        }
        private static void ExitCurrentUow()
        {
            var unitOfWorkKey = CallContext.LogicalGetData(ContextKey) as string;
            if (unitOfWorkKey.IsNull())
                return;
            IUnitOfWork unitOfWork;
            if (!UnitOfWorkDictionary.TryGetValue(unitOfWorkKey, out unitOfWork))
            {
                CallContext.FreeNamedDataSlot(ContextKey);
                return;
            }
            UnitOfWorkDictionary.TryRemove(unitOfWorkKey, out unitOfWork);
            if (unitOfWork.Outer == null)
            {
                CallContext.FreeNamedDataSlot(ContextKey);
                return;
            }
            var outerKey = unitOfWork.Outer.ID;
            if (!UnitOfWorkDictionary.TryGetValue(outerKey, out unitOfWork))
            {
                CallContext.FreeNamedDataSlot(ContextKey);
                return;
            }
            CallContext.LogicalSetData(ContextKey, outerKey);
        }
    }
}
