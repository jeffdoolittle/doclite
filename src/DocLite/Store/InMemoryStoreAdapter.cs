using System;
using System.Collections;
using System.Collections.Generic;

namespace DocLite.Store
{
    public class InMemoryStoreAdapter<TKey, TValue> : IStore<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Flush()
        {
            // no op
        }

        public TValue this[TKey index]
        {
            get { return _dictionary[index]; }
            set { _dictionary[index] = value; }
        }

        public void Dispose()
        {
            // no op
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}