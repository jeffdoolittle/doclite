using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace DocLite.Tests.Performance
{
    public interface ISessionFactoryFixture
    {
        ISessionFactory SessionFactory { get; }
    }

    public class when_storing_persistent_documents : IDisposable
    {
        private readonly SessionFactory _factory;

        public when_storing_persistent_documents()
        {
            _factory = new SessionFactory();
        }

        [Theory, InlineData(100), InlineData(1000), InlineData(10000), InlineData(100000)]
        public void then_multiple_documents_can_be_stored_and_retrieved(int documentCount)
        {
            var start = DateTime.UtcNow;

            Debug.WriteLine("Storing {0} documents - {1}", documentCount, Elapsed(start));

            using (var session = _factory.OpenSession())
            {
                for (int i = 0; i < documentCount; i++)
                {
                    session.Add(new TestDocument { Id = i + 1, Name = "Document " + (i + 1) });
                }
            }

            Debug.WriteLine("Stored {0} documents - {1}", documentCount, Elapsed(start));

            Debug.WriteLine("Retrieving {0} documents - {1}", documentCount, Elapsed(start));

            using (var session = _factory.OpenSession())
            {
                var matches = session.GetAll<TestDocument>().ToList();

                Assert.Equal(documentCount, matches.Count);
            }

            Debug.WriteLine("Retrieved {0} documents - {1}", documentCount, Elapsed(start));
        }

        private TimeSpan Elapsed(DateTimeOffset start)
        {
            return DateTime.UtcNow - start;
        }

        public void Dispose()
        {
            _factory.Dispose();
            _factory.DropStore();
        }
    }

    public class TestDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}