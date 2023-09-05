using System.Reactive.Concurrency;

namespace Shoutr.WebServer.Reactive;

public class SchedulerProvider
{
    private IScheduler _scheduler = new TaskPoolScheduler(new TaskFactory());
    public IScheduler Get(string name)
    {
        return _scheduler;
    }
}