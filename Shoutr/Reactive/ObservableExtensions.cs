using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Shoutr.Reactive
{
    public static class ObservableExtensions
    {
        public static IObservable<TOut> WhenStopped<TIn, TOut>(this IObservable<TIn> observable,
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