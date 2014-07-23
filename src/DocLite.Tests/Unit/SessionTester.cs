using System;
using System.Linq;
using Microsoft.Isam.Esent.Collections.Generic;
using Xunit;

namespace DocLite.Tests.Unit
{
    public class SessionTesterFixture : IDisposable
    {
        private readonly SessionFactory _factory;

        public const int DocumentCount = 10000;

        public SessionTesterFixture()
        {
            _factory = new SessionFactory();
            SessionFactory = _factory;

            using (var session = _factory.OpenSession())
            {
                for (int i = 0; i < DocumentCount; i++)
                {
                    session.Add(new TestDocument { Id = i + 1 });
                }
            }
        }

        public ISessionFactory SessionFactory { get; private set; }

        public void Dispose()
        {
            _factory.Dispose();
            PersistentDictionaryFile.DeleteFiles(_factory.Location);
        }
    }

    public class SessionTester : IUseFixture<SessionTesterFixture>
    {
        private SessionTesterFixture _fixture;
        private ISessionFactory _factory;
        private ISession _session;

        public void SetFixture(SessionTesterFixture fixture)
        {
            _fixture = fixture;
            _factory = _fixture.SessionFactory;
            _session = _factory.OpenSession();
        }

        [Fact]
        public void then_the_first_document_can_be_retrieved()
        {
            var first = _session.First<TestDocument>();

            Assert.Equal(1, first.Id);
        }

        [Fact]
        public void then_the_last_document_can_be_retrieved()
        {
            var last = _session.Last<TestDocument>();
            Assert.Equal(SessionTesterFixture.DocumentCount, last.Id);
        }

        [Fact]
        public void then_documents_can_be_retrieved_for_multiple_ids()
        {
            var matches = _session.Get<TestDocument>(new object[] {10, 50, 500, 999}).ToList();
            Assert.Equal(4, matches.Count);
            Assert.Equal(10, matches[0].Id);
            Assert.Equal(50, matches[1].Id);
            Assert.Equal(500, matches[2].Id);
            Assert.Equal(999, matches[3].Id);
        }

        [Fact]
        public void then_documents_can_be_retrieved_from_the_start_of_the_collection_with_skip_take()
        {
            var matches = _session.Get<TestDocument>(0, 10).ToList();
            Assert.Equal(10, matches.Count);
            Assert.Equal(1, matches.First().Id);
            Assert.Equal(10, matches.Last().Id);
        }

        [Fact]
        public void then_documents_can_be_retrieved_with_skip_take()
        {
            var matches = _session.Get<TestDocument>(500, 10).ToList();
            Assert.Equal(10, matches.Count);
            Assert.Equal(501, matches.First().Id);
            Assert.Equal(510, matches.Last().Id);
        }

        [Fact]
        public void then_documents_can_be_retrieved_from_the_end_of_the_collection_with_skip_take()
        {
            var docCount = SessionTesterFixture.DocumentCount;

            var matches = _session
                .Get<TestDocument>(docCount - 10, 10)
                .ToList();
            Assert.Equal(10, matches.Count);
            Assert.Equal(docCount - 9, matches.First().Id);
            Assert.Equal(docCount, matches.Last().Id);
        }

        [Fact]
        public void then_an_exception_is_thrown_when_skip_take_exceeds_the_collection_size()
        {
            var docCount = SessionTesterFixture.DocumentCount;

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    {
                        var matches = _session
                            .Get<TestDocument>(docCount - 10, 11)
                            .ToList();
                    });
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}