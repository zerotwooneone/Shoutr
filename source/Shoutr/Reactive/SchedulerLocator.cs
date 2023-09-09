using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace Shoutr.Reactive;

public class SchedulerLocator : ISchedulerLocator
{
    /// <summary>
    /// This will cancel all tasks created by the task pool which have not overridden their cancel token
    /// </summary>
    private readonly CancellationTokenSource _taskCanceller = new CancellationTokenSource();
    private readonly IScheduler _taskPoolScheduler;

    public SchedulerLocator()
    {
        _taskPoolScheduler = new TaskPoolScheduler(new TaskFactory(_taskCanceller.Token));
    }

    public IScheduler GetScheduler(string name)
    {
        //return Scheduler.Default;
        return _taskPoolScheduler;
    }

    public void Shutdown()
    {
        
    }
}