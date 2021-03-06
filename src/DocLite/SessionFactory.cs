﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using DocLite.Serialization;
using Microsoft.Isam.Esent.Collections.Generic;

namespace DocLite
{
    /// <summary>
    /// The Session Factory provides configuration options and acts as the factory for a doclite <see cref="ISession"/>
    /// </summary>
    public class SessionFactory : ISessionFactory
    {
        private PersistentDictionary<string, string> _store;
        private string _location = "data";
        private IDocumentSerializer _documentSerializer;
        private bool _shouldCompress;
        private byte[] _encryptionKey;

        /// <summary>
        /// Creates a <see cref="SessionFactory"/>
        /// </summary>
        /// <param name="configure">Optional configuration for a <see cref="SessionFactory"/></param>
        public SessionFactory(Action<SessionFactoryConfiguration> configure = null)
        {
            var configuration = new SessionFactoryConfiguration(this);
            if (configure != null)
            {
                configure(configuration);
            }

            ConfigurePersistentStore();
            ConfigurePersistentSerializer();
        }

        private void ConfigurePersistentStore()
        {
            _store = new PersistentDictionary<string, string>(_location);
        }

        private void ConfigureInMemorySerializer()
        {
            var serializer = new JsonSerializer();
            _documentSerializer = new JsonDocumentSerializer(serializer);
        }

        private void ConfigurePersistentSerializer()
        {
            var serializer = (ISerialize)new JsonSerializer();

            if (_shouldCompress)
                serializer = new GzipSerializer(serializer);

            if (_encryptionKey != null)
                serializer = new RijndaelSerializer(serializer, _encryptionKey);

            _documentSerializer = new ByteStreamDocumentSerializer(serializer);
        }

        /// <summary>
        /// Opens a new <see cref="ISession"/>
        /// </summary>
        /// <returns>A new <see cref="ISession"/></returns>
        public ISession OpenSession()
        {
            return new Session(_store, _documentSerializer);
        }

        /// <summary>
        /// The location of the persistence store backing the <see cref="SessionFactory"/>
        /// </summary>
        public string Location { get { return _location; } }

        /// <summary>
        /// Disposes of any resources in use by the persistence store
        /// </summary>
        public void Dispose()
        {
            _store.Dispose();
        }

        /// <summary>
        /// Drops the persistent store managed by this SessionFactory
        /// </summary>
        public void DropStore()
        {
            PersistentDictionaryFile.DeleteFiles(Location);
        }

        /// <summary>
        /// Configurer for a <see cref="SessionFactory"/>
        /// </summary>
        public class SessionFactoryConfiguration
        {
            private readonly SessionFactory _factory;

            protected internal SessionFactoryConfiguration(SessionFactory factory)
            {
                _factory = factory;
                _factory._location = Path.Combine(ExecutingAssemblyPath, "data");
            }

            /// <summary>
            /// Configures the location where <see cref="SessionFactory"/> documents will be persisted. Default is "data" folder in the path of the currently executing assembly.
            /// </summary>
            /// <param name="location"></param>
            /// <returns></returns>
            public SessionFactoryConfiguration StoreAt(string location)
            {
                _factory._location = location;
                return this;
            }

            /// <summary>
            /// Configures the <see cref="SessionFactory"/> to use Gzip compression
            /// </summary>
            /// <returns></returns>
            public SessionFactoryConfiguration Compress()
            {
                _factory._shouldCompress = true;
                return this;
            }

            /// <summary>
            /// Configures the <see cref="SessionFactory"/> to use Rijndael encryption with a 16 byte key.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public SessionFactoryConfiguration EncryptWithKey(string key)
            {
                _factory._encryptionKey = Encoding.UTF8.GetBytes(key);
                return this;
            }

            private static string ExecutingAssemblyPath
            {
                get
                {
                    var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                    var uri = new UriBuilder(codeBase);
                    var path = Uri.UnescapeDataString(uri.Path);
                    var directory = Path.GetDirectoryName(path);
                    return directory;
                }
            }
        }
    }
}