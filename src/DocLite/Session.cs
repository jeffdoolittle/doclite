using System;
using System.Collections.Generic;
using System.Linq;
using DocLite.Serialization;
using Microsoft.Isam.Esent.Collections.Generic;

namespace DocLite
{
    public interface ISession : IDisposable
    {
        T Get<T>(object id = null);
        IEnumerable<T> GetAll<T>();
        void Add(object document);
        void Remove(object document);
    }

    public class Session : ISession
    {
        private const string IdPropertyName = "Id";

        private readonly PersistentDictionary<string, string> _store;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IdHelper _idHelper;

        public Session(PersistentDictionary<string, string> store, IDocumentSerializer documentSerializer)
        {
            _store = store;
            _documentSerializer = documentSerializer;
            _idHelper = new IdHelper();
            _idHelper.Initialize(IdPropertyName);
        }

        public T Get<T>(object id = null)
        {
            var key = GetDocumentKey(typeof(T), id);
            if (!_store.ContainsKey(key)) return default(T);
            var value = _store[key];
            return _documentSerializer.Deserialize<T>(value);
        }

        public IEnumerable<T> GetAll<T>()
        {
            var key = GetDocumentName(typeof(T));
            return _store
                .Where(x => x.Key.StartsWith(key))
                .Select(x => _documentSerializer.Deserialize<T>(x.Value));
        }

        public void Add(object document)
        {
            _idHelper.EnsureAllGuidsIdsInObjectGraphAreNotEmpty(document);
            var key = GetDocumentKey(document);
            _store[key] = _documentSerializer.Serialize(document);
        }

        public void Remove(object document)
        {
            var key = GetDocumentKey(document);
            _store.Remove(key);
        }

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