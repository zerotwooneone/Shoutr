using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace Shoutr.Tests
{
    [TestFixture]
    public class BroadcasterTests
    {
        private TestScheduler _scheduler;
        [SetUp]
        public void Setup()
        {
            _scheduler = new TestScheduler();
        }

        [Test]
        public async Task BroadcastTest()
        {
            var bc = new Broadcaster();
            var serializedHeader = new byte[] { 1, 2, 3, 4};
            const int rebroadcastSeconds = 1;
            var payloadObservable = Observable.Empty<byte[]>();
            var observable = bc.GetHeaderObservable(serializedHeader,
                TimeSpan.FromSeconds(rebroadcastSeconds),
                payloadObservable,
                _scheduler);

            var headers = await observable.ToArray();
            CollectionAssert.AreEqual(serializedHeader, headers[0]);
            CollectionAssert.AreEqual(serializedHeader, headers[1]);
        }
    }
}