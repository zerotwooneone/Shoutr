using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using Shoutr.Reactive;

namespace Shoutr.Tests.Reactive;

public class TestSchedulerLocator: ISchedulerLocator
{
    public readonly TestScheduler TestScheduler;

    public TestSchedulerLocator()
    {
        TestScheduler = new TestScheduler();
    }

    public IScheduler GetScheduler(string name)
    {
        return TestScheduler;
    }
}