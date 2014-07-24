using System;
using System.Linq;
using Xunit;

namespace DocLite.Tests.Unit
{
    public class SequenceTests : IDisposable
    {
        private readonly SessionFactory _factory;

        public SequenceTests()
        {
            _factory = new SessionFactory();
        }

        [Fact]
        public void documents_with_integer_key_are_stored_in_key_sequence_when_added_in_sequence()
        {
            var doc1 = new DocumentWithIntegerKey { Id = 1 };
            var doc2 = new DocumentWithIntegerKey { Id = 2 };
            var doc3 = new DocumentWithIntegerKey { Id = 3 };

            using (var session = _factory.OpenSession())
            {
                session.Add(doc1);
                session.Add(doc2);
                session.Add(doc3);
            }

            using (var session = _factory.OpenSession())
            {
                var docs = session.GetAll<DocumentWithIntegerKey>().ToList();

                Assert.Equal(1, docs[0].Id);
                Assert.Equal(2, docs[1].Id);
                Assert.Equal(3, docs[2].Id);
            }
        }

        [Fact]
        public void documents_with_integer_key_are_stored_in_key_sequence_when_added_out_of_sequence()
        {
            var doc1 = new DocumentWithIntegerKey { Id = 1 };
            var doc2 = new DocumentWithIntegerKey { Id = 2 };
            var doc3 = new DocumentWithIntegerKey { Id = 3 };

            using (var session = _factory.OpenSession())
            {
                session.Add(doc3);
                session.Add(doc1);
                session.Add(doc2);
            }

            using (var session = _factory.OpenSession())
            {
                var docs = session.GetAll<DocumentWithIntegerKey>().ToList();

                Assert.Equal(1, docs[0].Id);
                Assert.Equal(2, docs[1].Id);
                Assert.Equal(3, docs[2].Id);
            }
        }

        public class DocumentWithIntegerKey
        {
            public int Id { get; set; }
        }

        public class DocumentWithGuidKey
        {
            public Guid Id { get; private set; }
        }

        public void Dispose()
        {
            _factory.Dispose();
            _factory.DropStore();
        }
    }
}