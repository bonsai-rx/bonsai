using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Configuration
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public abstract class SortedKeyedCollection<TKey, TItem> : ICollection<TItem>
    {
        readonly SortedList<TKey, TItem> items = new SortedList<TKey, TItem>();

        protected abstract TKey GetKeyForItem(TItem item);

        public TItem this[TKey key]
        {
            get { return items[key]; }
        }

        public int Count
        {
            get { return items.Count; }
        }

        bool ICollection<TItem>.IsReadOnly
        {
            get { return false; }
        }

        public void Add(TItem item)
        {
            var key = GetKeyForItem(item);
            items.Add(key, item);
        }

        public bool Contains(TKey key)
        {
            return items.ContainsKey(key);
        }

        bool Contains(TKey key, TItem item)
        {
            TItem existing;
            if (items.TryGetValue(key, out existing))
            {
                return EqualityComparer<TItem>.Default.Equals(item, existing);
            }

            return false;
        }

        public bool Contains(TItem item)
        {
            var key = GetKeyForItem(item);
            return Contains(key, item);
        }

        public bool Remove(TKey key)
        {
            return items.Remove(key);
        }

        public bool Remove(TItem item)
        {
            var key = GetKeyForItem(item);
            if (Contains(key, item))
            {
                return items.Remove(key);
            }

            return false;
        }

        public void Clear()
        {
            items.Clear();
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            items.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
