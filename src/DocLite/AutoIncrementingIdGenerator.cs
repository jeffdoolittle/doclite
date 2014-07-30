using System;

namespace DocLite
{
    internal class AutoIncrementingIdGenerator
    {
        private readonly ISession _session;
        private readonly IDocLiteConfiguration _configuration;

        internal AutoIncrementingIdGenerator(ISession session, IDocLiteConfiguration configuration)
        {
            _session = session;
            _configuration = configuration;
        }

        public void AssignId(object document)
        {
            var descriptor = _configuration.GetIdProperty(document.GetType());

            if (descriptor == null)
            {
                return;
            }

            var propertyType = descriptor.PropertyType;

            if (IsValid(propertyType))
            {
                var documentName = _configuration.GetDocumentName(document.GetType());
                var key = string.Format("NextAutoId-{0}", documentName);
                var nextAutoId = _session.Get<NextAutoId>(key) ?? new NextAutoId { Id = key };

                var nextId = Convert.ChangeType(++nextAutoId.NextId, propertyType);

                _session.Add(nextAutoId);

                ((IFlushable)_session).Flush();

                descriptor.SetValue(document, nextId, new object[0]);
            }
        }

        private bool IsValid(Type type)
        {
            return type == typeof(long)
                   || type == typeof(int)
                   || type == typeof(short)
                   || type == typeof(byte);
        }

        public class NextAutoId
        {
            public string Id { get; set; }
            public long NextId { get; set; }
        }
    }
}