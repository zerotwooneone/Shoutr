namespace Library.Reactive
{
    public class SchedulerWrapper : Interface.Reactive.IScheduler
    {
        public SchedulerWrapper(System.Reactive.Concurrency.IScheduler scheduler)
        {
            Scheduler = scheduler;
        }

        public System.Reactive.Concurrency.IScheduler Scheduler { get; }
    }
}