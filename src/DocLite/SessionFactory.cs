using System;
using System.Text;
using DocLite.Serialization;
using Microsoft.Isam.Esent.Collections.Generic;

namespace DocLite
{
    public interface ISessionFactory : IDisposable
    {
        ISession OpenSession();
    }

    public class SessionFactory : ISessionFactory
    {
        private bool _initialized;
        private PersistentDictionary<string, string> _store;
        private string _location = "data";
        private IDocumentSerializer _documentSerializer;
        private bool _shouldCompress;
        private byte[] _encryptionKey;

        public void StoreAt(string location)
        {
            GuardNotInitialized();
            _location = location;
        }

        public void Compress()
        {
            GuardNotInitialized();
            _shouldCompress = true;
        }

        public void EncryptWithKey(string key)
        {
            GuardNotInitialized();
            _encryptionKey = Encoding.UTF8.GetBytes(key);
        }

        private void GuardNotInitialized()
        {
            if (_initialized)
                throw new InvalidOperationException("Document store has already been initialized and cannot be modified.");
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(_location))
                throw new InvalidOperationException("Document store must have a Location value set prior to initialization");

            _initialized = true;

            ConfigurePersistentStore();
            ConfigureSerializer();
        }

        private void ConfigurePersistentStore()
        {
            _store = new PersistentDictionary<string, string>(_location);
        }

        private void ConfigureSerializer()
        {
            var serializer = (ISerialize)new JsonSerializer();

            if (_shouldCompress)
                serializer = new GzipSerializer(serializer);
            
            if (_encryptionKey != null)
                serializer = new RijndaelSerializer(serializer, _encryptionKey);

            _documentSerializer = new ByteStreamDocumentSerializer(serializer);
        }

        public ISession OpenSession()
        {
            return new Session(_store, _documentSerializer);
        }

        public void Dispose()
        {
            _store.Dispose();
        }
    }

}