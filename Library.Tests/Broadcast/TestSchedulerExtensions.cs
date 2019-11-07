using Microsoft.Reactive.Testing;
using System;

namespace Library.Tests.Broadcast
{
    public static class TestSchedulerExtensions
    {
        public static void AdvanceBy(this TestScheduler testScheduler, TimeSpan time)
        {
            testScheduler.AdvanceBy(time.Ticks);
        }
    }
}
