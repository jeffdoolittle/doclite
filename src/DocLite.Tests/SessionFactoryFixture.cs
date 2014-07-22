using System;
using System.IO;
using Microsoft.Isam.Esent.Collections.Generic;
using Xunit;

namespace DocLite.Tests
{
    public class SessionFactoryFixture : IDisposable
    {
        public SessionFactoryFixture()
        {
            Location = Path.Combine(AssemblyPath.ExecutingAssembly, "data");

            var store = new SessionFactory();
            store.StoreAt(Location);
            store.Compress();
            store.EncryptWithKey("ABCDEFGHIJKLMNOP");
            store.Initialize();

            SessionFactory = store;
        }

        public ISessionFactory SessionFactory { get; private set; }
        public string Location { get; private set; }

        public void Dispose()
        {
            SessionFactory.Dispose();
            PersistentDictionaryFile.DeleteFiles(Location);
        }
    }

    public class when_storing_a_simple_document : IUseFixture<SessionFactoryFixture>
    {
        private SessionFactoryFixture _fixture;

        public void SetFixture(SessionFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void then_the_document_can_be_retrieved()
        {
            using (var session = _fixture.SessionFactory.OpenSession())
            {
                session.Add(new MyDocument
                {
                    Id = 1,
                    Name = "foo bar",
                    Description = "something good!"
                });
            }

            using (var session = _fixture.SessionFactory.OpenSession())
            {
                var match = session.Get<MyDocument>(1);

                Assert.Equal(1, match.Id);
                Assert.Equal("foo bar", match.Name);
                Assert.Equal("something good!", match.Description);
            }
        }        
    }

    public class MyDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
