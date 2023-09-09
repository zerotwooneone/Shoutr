using System.Reactive.Concurrency;

namespace Shoutr.Reactive;

/// <summary>
/// Locates the appropriate reactive scheduler for a given situation
/// </summary>
public interface ISchedulerLocator
{
    /// <summary>
    /// Returns a scheduler given a purpose for the scheduler
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IScheduler GetScheduler(string name);
}