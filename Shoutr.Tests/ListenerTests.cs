using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace Shoutr.Tests
{
    public class ListenerTests
    {
        private TestScheduler _scheduler;
        [SetUp]
        public void Setup()
        {
            _scheduler = new TestScheduler();
        }

        /*[Test]
        public async Task Test1()
        {
            var listener = CreateListener();
            var timeout = TimeSpan.FromSeconds(1);
            var observable = Observable.Return(Observable.Return(1)).Concat(Observable.Never<IObservable<int>>());
            var obs = listener
                .CreateTimeoutObservable2(observable, x => x, timeout, _scheduler);
            var s = obs
                .ObserveOn(_scheduler)
                .Subscribe(x =>
            {
                Assert.Pass();
            });
            _scheduler.Start();
            Assert.Fail();
        }
        
        [Test]
        public async Task Test3()
        {
            var listener = CreateListener();
            var timeout = TimeSpan.FromSeconds(1);
            var observable = Observable.Return(Observable.Return(1)).Concat(Observable.Never<IObservable<int>>());
            var obs = listener
                .CreateTimeoutObservable3(observable, x => x, timeout, _scheduler);
            var s = obs
                .ObserveOn(_scheduler)
                .Subscribe(x =>
                {
                    Assert.Pass();
                });
            _scheduler.Start();
            Assert.Fail();
        }
        
        [Test]
        public async Task Test2()
        {
            var timeout = TimeSpan.FromSeconds(1);
            var observable = Observable.Return(1).Delay(timeout, _scheduler);
            var s = observable
                //.ObserveOn(_scheduler)
                .Subscribe(x =>
                {
                    Assert.Pass();
                });
            _scheduler.Start();
            Assert.Fail();
        }*/

        private Listener CreateListener()
        {
            return new Listener();
        }
    }
}