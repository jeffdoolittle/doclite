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
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Gets a document by Type and Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(object id = null);

        /// <summary>
        /// Gets all documents for a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetAll<T>();

        /// <summary>
        /// Adds a document to the session
        /// </summary>
        /// <param name="document"></param>
        void Add(object document);
        
        /// <summary>
        /// Removes a document from the session
        /// </summary>
        /// <param name="document"></param>
        void Remove(object document);
    }

    /// <summary>
    /// 
    /// </summary>
    public class Session : ISession
    {
        private const string IdPropertyName = "Id";

        private readonly IStore<string, string> _store;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IdHelper _idHelper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="store"></param>
        /// <param name="documentSerializer"></param>
        public Session(IStore<string, string> store, IDocumentSerializer documentSerializer)
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
            if (!_store.ContainsKey(key)) return default(T);
            var value = _store[key];
            return _documentSerializer.Deserialize<T>(value);
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
                .Select(x => _documentSerializer.Deserialize<T>(x.Value));
        }

        /// <summary>
        /// Adds a document to the persistent store
        /// </summary>
        /// <param name="document"></param>
        public void Add(object document)
        {
            _idHelper.EnsureAllGuidsIdsInObjectGraphAreNotEmpty(document);
            var key = GetDocumentKey(document);
            _store[key] = _documentSerializer.Serialize(document);
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
        /// Flushes changes to disk
        /// </summary>
        public void Dispose()
        {
            _store.Flush();
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

            return id == null ? documentName : string.Format("{0}-{1}", documentName, id);
        }
    }
}