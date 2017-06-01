using System;

namespace Tzen.Framework.Ioc
{
    public static class SingleFactory<T>
    {
        private static readonly Lazy<T> LazyInstance;

        public static T Instance
        {
            get
            {
                return LazyInstance.Value;
            }
        }
        static SingleFactory()
        {
            LazyInstance = new Lazy<T>(() => IocManager.Instance.Resolve<T>());
        }
    }
}
