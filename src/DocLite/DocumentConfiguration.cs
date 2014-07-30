using System;
using System.Reflection;

namespace DocLite
{
    public interface IDocLiteConfiguration
    {
        string IdPropertyName { get; }
        string GetDocumentName(Type type);
        PropertyInfo GetIdProperty(Type type);
    }

    public class DocLiteConfiguration : IDocLiteConfiguration
    {
        private string _idPropertyName = "Id";

        public string IdPropertyName { get { return _idPropertyName; } }

        public DocLiteConfiguration(Action<DocLiteConfigurationConfigurer> configure)
        {
            var configuration = new DocLiteConfigurationConfigurer(this);
            configure(configuration);
        }

        public string GetDocumentName(Type type)
        {
            return type.Name;
        }

        public PropertyInfo GetIdProperty(Type type)
        {
            var descriptor = type.GetProperty(IdPropertyName,
                                              BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance);

            return descriptor;
        }

        public class DocLiteConfigurationConfigurer
        {
            private readonly DocLiteConfiguration _configuration;

            protected internal DocLiteConfigurationConfigurer(DocLiteConfiguration configuration)
            {
                _configuration = configuration;
            }
        }
    }

    public class DocumentConfiguration
    {
        private Type _documentType;
        private bool _autoGenerateIds;

        public DocumentConfiguration(Action<DocumentConfigurationConfigurer> configure)
        {
            var configurer = new DocumentConfigurationConfigurer(this);
            configure(configurer);
        }

        public Type DocumentType
        {
            get { return _documentType; }
        }

        public bool AutoGenerateIds
        {
            get { return _autoGenerateIds; }
        }

        public class DocumentConfigurationConfigurer
        {
            private readonly DocumentConfiguration _configuration;

            protected internal DocumentConfigurationConfigurer(DocumentConfiguration configuration)
            {
                _configuration = configuration;
            }

            public DocumentConfigurationConfigurer For<T>()
            {
                _configuration._documentType = typeof(T);
                return this;
            }

            public DocumentConfigurationConfigurer AutoId
            {
                get { return this; }
            }

            public DocumentConfigurationConfigurer ManualId
            {
                get { return this; }
            }


        }
    }
}