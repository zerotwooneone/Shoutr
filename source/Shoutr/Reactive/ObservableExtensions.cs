using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Shoutr.Reactive
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Repeats the given value every timeout until the source completes
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <param name="scheduler"></param>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        public static IObservable<TOut> RepeatWhile<TIn, TOut>(this IObservable<TIn> observable,
            TOut value,
            TimeSpan timeout,
            IScheduler scheduler)
        {
            var timeoutObservable = Observable
                .Return(value, scheduler)
                .Delay(timeout, scheduler)
                .Concat(Observable.Never<TOut>())
                .TakeUntil(observable)
                .Repeat();
            return timeoutObservable;

        }
    }
}