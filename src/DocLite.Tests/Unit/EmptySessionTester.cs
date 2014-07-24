using System;
using System.Linq;
using Xunit;

namespace DocLite.Tests.Unit
{
    public class EmptySessionTester : IDisposable
    {
        private readonly SessionFactory _factory;
        private readonly ISession _session;

        public EmptySessionTester()
        {
            _factory = new SessionFactory();
            _session = _factory.OpenSession();
        }

        [Fact]
        public void fetching_first_or_default_returns_null()
        {
            var match = _session.FirstOrDefault<FakeDoc>();
            Assert.Null(match);
        }

        [Fact]
        public void fetching_last_or_default_returns_null()
        {
            var match = _session.LastOrDefault<FakeDoc>();
            Assert.Null(match);
        }

        [Fact]
        public void fetching_single_or_default_returns_null()
        {
            var match = _session.SingleOrDefault<FakeDoc>();
            Assert.Null(match);
        }

        [Fact]
        public void skip_take_returns_empty_enumerable()
        {
            var matches = _session.Get<FakeDoc>(0, 10).ToList();
            Assert.Equal(0, matches.Count);
        }

        [Fact]
        public void get_all_returns_empty_enumerable()
        {
            var matches = _session.GetAll<FakeDoc>().ToList();
            Assert.Equal(0, matches.Count);
        }

        [Fact]
        public void get_without_id_returns_null()
        {
            var match = _session.Get<FakeDoc>();
            Assert.Null(match);
        }

        [Fact]
        public void get_by_single_id_returns_null()
        {
            var match = _session.Get<FakeDoc>(5);
            Assert.Null(match);
        }

        [Fact]
        public void get_by_multi_id_returns_empty_enumerable()
        {
            var matches = _session.Get<FakeDoc>(new object[]{5,10}).ToList();
            Assert.Equal(0, matches.Count);
        }

        public void Dispose()
        {
            _session.Dispose();
            _factory.Dispose();
            _factory.DropStore();
        }

        public class FakeDoc
        {
            public int Id { get; set; }
        }
    }
}