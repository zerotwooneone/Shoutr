using System.Reactive.Concurrency;

namespace Library.Reactive
{
    public interface ISchedulerProvider
    {
        IScheduler Default {get;}
    }
}
