using System;
using System.Reactive.Linq;
using Library.Interface.Reactive;

namespace Library.Reactive
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Produces an observable which will emit no more frequently than once per timeBetweenEmits. This has the shortcoming that if the source observable does not produce
        /// before that timespan then multiple source values will be emited to "catch up". 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="timeBetweenEmits"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static IObservable<T> TickingThrottle<T>(this IObservable<T> source,
            TimeSpan timeBetweenEmits,
            Interface.Reactive.IScheduler scheduler)
        {
            var interval = Observable.Interval(timeBetweenEmits, ((SchedulerWrapper)scheduler).Scheduler);

            var firstOnly = source.Take(1);
            var allOthers = source.Skip(1);

            var throttled = allOthers.Zip(interval, (t, intervalValue) => t);

            return firstOnly
                .Merge(throttled);
        }

        /// <summary>
        /// Merges elements from two observable sequences into a single observable sequence, using the specified scheduler for enumeration of and subscription to the sources.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="first">First observable sequence.</param>
        /// <param name="second">Second observable sequence.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given sequences.</param>
        /// <returns>The observable sequence that merges the elements of the given sequences.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> or <paramref name="scheduler"/> is null.</exception>
        public static IObservable<TSource> Merge<TSource>(this IObservable<TSource> first, IObservable<TSource> second, IScheduler scheduler)
        {
            return first.Merge(second, ((SchedulerWrapper)scheduler).Scheduler);
        }

        /// <summary>
        /// Returns an observable sequence that periodically produces a value after the specified initial relative due time has elapsed, using the specified scheduler to run timers.
        /// </summary>
        /// <param name="dueTime">Relative time at which to produce the first value. If this value is less than or equal to TimeSpan.Zero, the timer will fire as soon as possible.</param>
        /// <param name="period">Period to produce subsequent values. If this value is equal to TimeSpan.Zero, the timer will recur as fast as possible.</param>
        /// <param name="scheduler">Scheduler to run timers on.</param>
        /// <returns>An observable sequence that produces a value after due time has elapsed and then each period.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is less than TimeSpan.Zero.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is null.</exception>
        public static IObservable<long> Timer(TimeSpan dueTime, TimeSpan period, IScheduler scheduler)
        {
            if (period < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(period));
            }

            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            return Observable.Timer(dueTime, period, ((SchedulerWrapper)scheduler).Scheduler);
        }
    }
}