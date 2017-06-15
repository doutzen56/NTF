using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NTF.Provider
{
    /// <summary>
    /// <see cref="IGrouping{TKey, TElement}"/>的简单实现
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TElement">值类型</typeparam>
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        TKey key;
        IEnumerable<TElement> group;

        public Grouping(TKey key, IEnumerable<TElement> group)
        {
            this.key = key;
            this.group = group;
        }

        public TKey Key
        {
            get { return this.key; }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            if (!(group is List<TElement>))
                group = group.ToList();
            return this.group.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.group.GetEnumerator();
        }
    }   
}