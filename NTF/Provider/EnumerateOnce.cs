using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NTF.Provider
{
    /// <summary>
    /// 定义一个只允许原子操作迭代的集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerateOnce<T> : IEnumerable<T>, IEnumerable
    {
        IEnumerable<T> enumerable;

        public EnumerateOnce(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var en = Interlocked.Exchange(ref enumerable, null);
            if (en != null)
            {
                return en.GetEnumerator();
            }
            throw new Exception("迭代器被多次迭代");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}