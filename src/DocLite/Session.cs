using System;
using System.Collections.Generic;
using System.Linq;
using DocLite.Serialization;
using DocLite.Store;

namespace DocLite
{
    /// <summary>
    /// A Session enables the retrieval, addition, and removal of documents from a backing store (persistent, or in memory)
    /// </summary>
    internal class Session : ISession
    {
        private const string IdPropertyName = "Id";

        private readonly IStore<string, string> _store;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IdHelper _idHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="store"></param>
        /// <param name="documentSerializer"></param>
        internal Session(IStore<string, string> store, IDocumentSerializer documentSerializer)
        {
            _store = store;
            _documentSerializer = documentSerializer;
            _idHelper = new IdHelper();
            _idHelper.Initialize(IdPropertyName);
        }

        /// <summary>
        /// Gets a document by Type and Id from the persistent store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get<T>(object id = null)
        {
            var key = GetDocumentKey(typeof(T), id);
            return Get<T>(key);
        }

        /// <summary>
        /// Gets documents matching Type and Ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(object[] ids)
        {
            var keys = ids.Select(x => GetDocumentKey(typeof (T), x));
            return Get<T>(keys);
        }

        /// <summary>
        /// Gets all documents for a given Type from the persistent store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetAll<T>()
        {
            var key = GetDocumentName(typeof(T));
            return _store
                .Where(x => x.Key.StartsWith(key))
                .Select(x => Deserialize<T>(x.Value));
        }

        /// <summary>
        /// Adds a document to the persistent store
        /// </summary>
        /// <param name="document"></param>
        public void Add(object document)
        {
            _idHelper.EnsureAllGuidsIdsInObjectGraphAreNotEmpty(document);
            var key = GetDocumentKey(document);
            _store[key] = Serialize(document);
        }

        /// <summary>
        /// Removes a document from the persistent store
        /// </summary>
        /// <param name="document"></param>
        public void Remove(object document)
        {
            var key = GetDocumentKey(document);
            _store.Remove(key);
        }

        /// <summary>
        /// Returns the first document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T First<T>()
        {
            var name = GetDocumentName(typeof (T));
            var key = _store.Keys.FirstOrDefault(x => x.StartsWith(name));
            return Get<T>(key);
        }

        /// <summary>
        /// Returns the last document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Last<T>()
        {
            var name = GetDocumentName(typeof(T));
            var key = _store.Keys.LastOrDefault(x => x.StartsWith(name));
            return Get<T>(key);
        }

        /// <summary>
        /// Returns a paged enumerable for documents of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(int skip, int take)
        {
            if (_store.Count == 0)
            {
                yield break;
            }

            for (int i = 0; i < take; i++)
            {
                var index = skip + i;
                var key = _store[index];
                yield return Get<T>(key);
            }
        }

        /// <summary>
        /// Flushes changes to disk
        /// </summary>
        public void Dispose()
        {
            _store.Flush();
        }

        private T Get<T>(string key)
        {
            if (!_store.ContainsKey(key)) return default(T);
            var value = _store[key];
            return Deserialize<T>(value);
        }

        private IEnumerable<T> Get<T>(IEnumerable<string> keys)
        {
            var list = keys.ToList();
            return _store.Where(x => list.Contains(x.Key))
                         .Select(x => Deserialize<T>(x.Value));
        }

        private string Serialize<T>(T document)
        {
            return _documentSerializer.Serialize(document);
        }

        private T Deserialize<T>(string value)
        {
            return _documentSerializer.Deserialize<T>(value);
        }

        private string GetDocumentName(Type type)
        {
            return type.Name;
        }

        private string GetDocumentKey(object document)
        {
            var id = _idHelper.GetId(document);
            return GetDocumentKey(document.GetType(), id);
        }

        private string GetDocumentKey(Type type, object id)
        {
            var documentName = GetDocumentName(type);

            var stringId = GetStringId(id);

            return stringId == null ? documentName : string.Format("{0}-{1}", documentName, stringId);
        }

        private string GetStringId(object id)
        {
            if (id is long)
            {
                return id.ToString().PadLeft(19, '0');
            }
            if (id is int)
            {
                return id.ToString().PadLeft(10, '0');
            }
            if (id is short)
            {
                return id.ToString().PadLeft(5, '0');
            }
            if (id is byte)
            {
                return id.ToString().PadLeft(3, '0');
            }
            if (id == null)
            {
                return null;
            }

            return id.ToString();
        }
    }
}