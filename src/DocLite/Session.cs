using System;
using System.Collections.Generic;
using System.Linq;
using DocLite.Serialization;
using Microsoft.Isam.Esent.Collections.Generic;

namespace DocLite
{
    /// <summary>
    /// A Session enables the retrieval, addition, and removal of documents from a backing store (persistent, or in memory)
    /// </summary>
    internal class Session : ISession
    {
        private const string IdPropertyName = "Id";

        private readonly PersistentDictionary<string, string> _store;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IdHelper _idHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="store"></param>
        /// <param name="documentSerializer"></param>
        internal Session(PersistentDictionary<string, string> store, IDocumentSerializer documentSerializer)
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
        public T Get<T>(object id)
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
            var keys = ids.Select(x => GetDocumentKey(typeof(T), x));
            return Get<T>(keys);
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

            var keys = _store
                .Skip(skip)
                .Take(take)
                .Select(x => x.Key);

            foreach (var key in keys)
            {
                yield return Get<T>(key);
            }
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
            PerformIdGeneration(document);
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
            var name = GetDocumentName(typeof(T));
            var key = _store.Keys.First(x => x.StartsWith(name));
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
            var key = _store.Keys.Last(x => x.StartsWith(name));
            return Get<T>(key);
        }

        /// <summary>
        /// Returns a single document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Single<T>()
        {
            var name = GetDocumentName(typeof(T));
            var key = _store.Keys.Single(x => x.StartsWith(name));
            return Get<T>(key);
        }

        /// <summary>
        /// Returns the first document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FirstOrDefault<T>()
        {
            var name = GetDocumentName(typeof(T));
            var key = _store.Keys.FirstOrDefault(x => x.StartsWith(name));
            return Get<T>(key);
        }

        /// <summary>
        /// Returns the last document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LastOrDefault<T>()
        {
            var name = GetDocumentName(typeof(T));
            var key = _store.Keys.LastOrDefault(x => x.StartsWith(name));
            return Get<T>(key);
        }

        /// <summary>
        /// Returns a single document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T SingleOrDefault<T>()
        {
            var name = GetDocumentName(typeof(T));
            var key = _store.Keys.SingleOrDefault(x => x.StartsWith(name));
            return Get<T>(key);
        }

        /// <summary>
        /// Returns true if documents exist of the specified Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Any<T>()
        {
            var name = GetDocumentName(typeof(T));
            return _store.Keys.Any(x => x.StartsWith(name));
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="Session"/> for the specified Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Count<T>()
        {
            var name = GetDocumentName(typeof(T));
            return _store.Keys.Count(x => x.StartsWith(name));
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

        private string GetDocumentName(Type type)
        {
            return type.Name;
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

        private void PerformIdGeneration(object document)
        {
            _idHelper.EnsureAllGuidsIdsInObjectGraphAreNotEmpty(document);

            if (_idHelper.IdType(document) == typeof (int)
                && (int)_idHelper.GetId(document) == default(int))
            {
                var name = GetDocumentName(document.GetType());
                var key = _store.Keys.LastOrDefault(x => x.StartsWith(name));

                int nextId = 1;

                if (key != null)
                {
                    var parts = key.Split('-');
                    var idString = parts[1];

                    int id;

                    if (int.TryParse(idString, out id))
                    {
                        nextId = id + 1;
                    }
                }

                _idHelper.SetId(document, nextId);
            }
        }
    }
}