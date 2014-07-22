using System;
using System.Collections.Generic;

namespace DocLite.Store
{
    public interface IStore<TKey, TValue> : 
        IDisposable,
        IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : IComparable<TKey>
    {
        bool Remove(TKey key);
        bool ContainsKey(TKey key);
        void Flush();

        TValue this[TKey index]
        {
            get;
            set;
        }
    }
}
