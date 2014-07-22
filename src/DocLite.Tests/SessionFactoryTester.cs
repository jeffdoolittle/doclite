using System;
using System.IO;
using System.Linq;
using Microsoft.Isam.Esent.Collections.Generic;
using Xunit;

namespace DocLite.Tests
{
    public interface ISessionFactoryFixture
    {
        ISessionFactory SessionFactory { get; }
    }

    public class InMemorySessionFactoryFixture : IDisposable, ISessionFactoryFixture
    {
        public InMemorySessionFactoryFixture()
        {
            var store = new SessionFactory(cfg => cfg.InMemory());

            SessionFactory = store;
        }

        public ISessionFactory SessionFactory { get; private set; }

        public void Dispose()
        {
            SessionFactory.Dispose();
        }
    }

    public class SessionFactoryFixture : IDisposable, ISessionFactoryFixture
    {
        private readonly SessionFactory _factory;

        public SessionFactoryFixture()
        {
            _factory = new SessionFactory(cfg => cfg
                                                     .Compress()
                                                     .EncryptWithKey("ABCDEFGHIJKLMNOP"));

            SessionFactory = _factory;
        }

        public ISessionFactory SessionFactory { get; private set; }

        public void Dispose()
        {
            SessionFactory.Dispose();
            PersistentDictionaryFile.DeleteFiles(_factory.Location);
        }
    }

    public abstract class SessionFactoryTester<T> : IUseFixture<T> where T : ISessionFactoryFixture, new()
    {
        private ISessionFactoryFixture _fixture;

        public void SetFixture(T fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void then_a_single_document_can_be_stored_and_retrieved()
        {
            using (var session = _fixture.SessionFactory.OpenSession())
            {
                session.Add(new TestDocument
                {
                    Id = 1,
                    Name = "foo bar",
                    Description = "something good!"
                });
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var match = session.Get<TestDocument>(1);

                Assert.Equal(1, match.Id);
                Assert.Equal("foo bar", match.Name);
                Assert.Equal("something good!", match.Description);
            }
        }

        [Fact]
        public void then_multiple_documents_can_be_stored_and_retrieved()
        {
            using (var session = _fixture.SessionFactory.OpenSession())
            {
                session.Add(new TestDocument
                {
                    Id = 1,
                    Name = "foo bar",
                    Description = "something good!"
                });

                session.Add(new TestDocument
                {
                    Id = 2,
                    Name = "foo bar",
                    Description = "something good!"
                });
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var matches = session.GetAll<TestDocument>();

                Assert.Equal(2, matches.Count());
            }
        }

        [Fact]
        public void then_removed_documents_are_not_retrievable()
        {
            using (var session = _fixture.SessionFactory.OpenSession())
            {
                session.Add(new TestDocument
                {
                    Id = 1,
                    Name = "foo bar",
                    Description = "something good!"
                });

                session.Add(new TestDocument
                {
                    Id = 2,
                    Name = "foo bar",
                    Description = "something good!"
                });
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var match = session.Get<TestDocument>(2);
                session.Remove(match);
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var matches = session.GetAll<TestDocument>();

                Assert.Equal(1, matches.Count());
            }
        }

        [Fact]
        public void then_documents_with_private_empty_guids_have_their_guids_assigned()
        {
            Guid id;

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var doc = new DocumentWithGuid();
                doc.SetName("foo bar");
                doc.SomeOtherProperty = "backing field with different name";
                session.Add(doc);
                id = doc.Id;
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var match = session.Get<DocumentWithGuid>(id);

                Assert.NotEqual(Guid.Empty, match.Id);
                Assert.Equal(id, match.Id);
                Assert.Equal("foo bar", match.Name);
                Assert.Equal("backing field with different name", match.SomeOtherProperty);
            }
        }

        [Fact]
        public void then_a_document_with_no_id_can_be_retrieved()
        {
            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var doc = new SingletonDocument();
                doc.SetName("foo bar");
                session.Add(doc);
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var match = session.Get<SingletonDocument>();

                Assert.Equal("foo bar", match.Name);
            }
        }

        [Fact]
        public void then_a_document_with_no_id_can_be_overwritten()
        {
            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var doc = new SingletonDocument();
                doc.SetName("first");
                session.Add(doc);
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var doc = new SingletonDocument();
                doc.SetName("second");
                session.Add(doc);
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var match = session.Get<SingletonDocument>();

                Assert.Equal("second", match.Name);
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var matches = session.GetAll<SingletonDocument>();

                Assert.Equal(1, matches.Count());
            }
        }
    }

    public class when_storing_in_memory_documents : SessionFactoryTester<InMemorySessionFactoryFixture>
    {
    }

    public class when_storing_persistent_documents : SessionFactoryTester<SessionFactoryFixture>
    {
    }
}