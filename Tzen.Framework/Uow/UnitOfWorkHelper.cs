using System;
using System.Reflection;

namespace Tzen.Framework.Uow
{
    internal static class UnitOfWorkHelper
    {
        public static bool HasUnitOfWorkAttribute(MemberInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(UnitOfWorkAttribute), true);
        }
        public static UnitOfWorkAttribute GetUnitOfWorkAttributeOrNull(MemberInfo methodInfo)
        {
            var attrs = methodInfo.GetCustomAttributes(typeof(UnitOfWorkAttribute), false);
            if (attrs.Length <= 0)
            {
                return null;
            }

            return attrs[0] as UnitOfWorkAttribute;
        }
        public static bool IsUowClass(Type type)
        {
            return false;
        }
    }
}
