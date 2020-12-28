using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Shoutr.Reactive;

namespace Shoutr.Tests.Reactive
{
    public class ObservableExtensionsTests
    {
        private TestScheduler _scheduler;
        [SetUp]
        public void Setup()
        {
            _scheduler = new TestScheduler();
        }

        [Test]
        public void WhenStopped_DoesNotEmitUntilTimeout_Never()
        {
            var observable = Observable.Never<int>();
            var timeout = TimeSpan.FromSeconds(1);
            var timeoutMinusOne = timeout.Subtract(TimeSpan.FromTicks(1));

            const int expected = -999;
            var result = observable.WhenStopped(expected, timeout, _scheduler);

            var didEmit = false;
            int? actual = null;
            result.Subscribe(v =>
            {
                didEmit = true;
                actual = v;
            });
            _scheduler.AdvanceBy(timeoutMinusOne.Ticks);
            Assert.IsFalse(didEmit);
            
            _scheduler.AdvanceBy(2);
            Assert.IsTrue(didEmit);
            Assert.AreEqual(expected,actual);
        }
    }
}