using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

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
            IScheduler scheduler)
        {
            var interval = Observable.Interval(timeBetweenEmits, scheduler);

            var firstOnly = source.Take(1);
            var allOthers = source.Skip(1);

            var throttled = allOthers.Zip(interval, (t, intervalValue) => t);

            return firstOnly
                .Merge(throttled);
        }
    }
}