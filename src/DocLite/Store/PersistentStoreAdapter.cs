using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Isam.Esent.Collections.Generic;

namespace DocLite.Store
{
    internal class PersistentStoreAdapter<TKey, TValue> : IStore<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private readonly PersistentDictionary<TKey, TValue> _dictionary;

        public PersistentStoreAdapter(PersistentDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

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
            _dictionary.Flush();
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
            _dictionary.Dispose();
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