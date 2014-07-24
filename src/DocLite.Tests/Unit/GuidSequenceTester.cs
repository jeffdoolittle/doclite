using System;
using Xunit;

namespace DocLite.Tests.Unit
{
    public class GuidSequenceTester : IDisposable
    {
        private SessionFactory _factory;
        private ISession _session;

        public GuidSequenceTester()
        {
            _factory = new SessionFactory();
            using (var session = _factory.OpenSession())
            {
                for (int i = 0; i <= 10000; i++)
                {
                    session.Add(new DocumentWithGuid());
                }
            }
            _session = _factory.OpenSession();
        }

        [Fact]
        public void auto_assigned_guids_are_sequential()
        {
            var last = new Guid();
            foreach (var item in _session.GetAll<DocumentWithGuid>())
            {
                Assert.True(item.Id.CompareTo(last) > 0);
                last = item.Id;
            }
        }

        public void Dispose()
        {
            _session.Dispose();
            _factory.Dispose();
            _factory.DropStore();
        }
    }
}