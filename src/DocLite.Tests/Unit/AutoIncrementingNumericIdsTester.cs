using System;
using Xunit;

namespace DocLite.Tests.Unit
{
    public class AutoIncrementingNumericIdsTester : IDisposable
    {
        private readonly SessionFactory _factory;
        private readonly ISession _session;

        public AutoIncrementingNumericIdsTester()
        {
            _factory = new SessionFactory();
            using (var session = _factory.OpenSession())
            {
                for (int i = 0; i < 10000; i++)
                {
                    session.Add(new MyDocument());
                }
            }
            _session = _factory.OpenSession();
        }

        [Fact]
        public void integer_ids_are_auto_generated()
        {
            Assert.Equal(10000, _session.Count<MyDocument>());
        }

        public void Dispose()
        {
            _session.Dispose();
            _factory.Dispose();
            _factory.DropStore();
        }

        public class MyDocument
        {
            public int Id { get; private set; }
        }
    }
}