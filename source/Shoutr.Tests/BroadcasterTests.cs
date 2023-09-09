using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Shoutr.Tests.Reactive;

namespace Shoutr.Tests
{
    [TestFixture]
    public class BroadcasterTests
    {
        private TestSchedulerLocator _schedulerLocator;
        private TestScheduler _scheduler;
        [SetUp]
        public void Setup()
        {
            _schedulerLocator = new TestSchedulerLocator();
            _scheduler = _schedulerLocator.TestScheduler;
        }

        [Test]
        public async Task GetHeaderObservable_ReturnsFirstAndLast_WhenOnePayloads()
        {   
            var bc = new Broadcaster(_schedulerLocator);
            var serializedHeader = new byte[] { 1, 2, 3, 4};
            const int rebroadcastSeconds = 1;
            var payloadObservable = Observable.Return(new byte[] { 9, 9, 9});
            var observable = bc.GetHeaderObservable(serializedHeader,
                TimeSpan.FromSeconds(rebroadcastSeconds),
                payloadObservable);

            var headers = await observable.ToArray();
            Assert.AreEqual(2, headers.Length);
            CollectionAssert.AreEqual(serializedHeader, headers[0]);
            CollectionAssert.AreEqual(serializedHeader, headers[1]);
        }
        
        [Test]
        public void GetHeaderObservable_ReturnsExtra_WhenOnePayloadDelayed()
        {   
            var bc = new Broadcaster(_schedulerLocator);
            var serializedHeader = new byte[] { 1, 2, 3, 4};
            var rebroadcastSeconds = TimeSpan.FromSeconds(1);
            var payloadObservable = Observable
                .Return(new byte[] { 9, 9, 9})
                .Delay(rebroadcastSeconds.Add(TimeSpan.FromTicks(1)), _scheduler)
                .Concat(Observable.Never<byte[]>());

            byte[] mostRecent = null;
            int count = 0;
            var sub = bc.GetHeaderObservable(serializedHeader,
                    rebroadcastSeconds,
                payloadObservable)
                .Subscribe(b =>
                {
                    mostRecent = b;
                    count++;
                });
            
            CollectionAssert.AreEqual(serializedHeader, mostRecent);
            Assert.AreEqual(1, count);
            
            _scheduler.AdvanceBy(rebroadcastSeconds);

            CollectionAssert.AreEqual(serializedHeader, mostRecent);
            Assert.AreEqual(2, count);
        }
    }
}