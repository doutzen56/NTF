using System;

namespace Tzen.Framwork.Ioc
{
    public static class SingletonDependency<T>
    {
        private static readonly Lazy<T> LazyInstance;

        public static Lazy<T> Instance
        {
            get
            {
                return LazyInstance;
            }
        }
        static SingletonDependency()
        {
            LazyInstance = new Lazy<T>(() => IocManager.Instance.Resolve<T>());
        }
    }
}
