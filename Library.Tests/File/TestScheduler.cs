using System;
using Library.Reactive;
using Library.Tests.Reactive;

namespace Library.Tests.File
{
    public class TestScheduler : SchedulerWrapper
    {
        public TestScheduler() : base(new Microsoft.Reactive.Testing.TestScheduler())
        {
        }

        public void Start()
        {
            ((Microsoft.Reactive.Testing.TestScheduler)Scheduler).Start();
        }

        public void AdvanceBy(TimeSpan timeSpan)
        {
            ((Microsoft.Reactive.Testing.TestScheduler)Scheduler).AdvanceBy(timeSpan);
        }

        public bool IsEnabled =>((Microsoft.Reactive.Testing.TestScheduler)Scheduler).IsEnabled;
        
    }
}
