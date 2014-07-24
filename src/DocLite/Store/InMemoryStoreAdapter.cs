using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DocLite.Store
{
    internal class InMemoryStoreAdapter<TKey, TValue> : IStore<TKey, TValue>
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

        TKey IStore<TKey, TValue>.this[int index]
        {
            get { return _dictionary.Keys.ElementAt(index); }
        }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        public ICollection<TKey> Keys { get { return _dictionary.Keys; } }
        
        public int Count { get { return _dictionary.Count; } }

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