using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NTF.Provider
{
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