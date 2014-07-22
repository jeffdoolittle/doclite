using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Isam.Esent.Collections.Generic;

namespace DocLite.Store
{
    public class PersistentStoreAdapter<TKey, TValue> : IStore<TKey, TValue>
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

        public TValue this[TKey index]
        {
            get { return _dictionary[index]; }
            set { _dictionary[index] = value; }
        }

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