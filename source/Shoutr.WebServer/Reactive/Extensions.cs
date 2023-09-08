using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Shoutr.WebServer.Reactive;

public static class Extensions
{
    public static void OnNext(this Subject<Unit> subject)
    {
        subject.OnNext(Unit.Default);
    }

    public static IDisposable SubscribeAsync<T>(
        this IObservable<T> observable,
        Func<T,Task> handler,
        Action<Exception>? errorHandler = null)
    {
        var innerExecptionHandler = errorHandler ?? SilentlySwallowErrors;
        
        IObservable<Unit> CreateObservable()
        {
            return observable
                .SelectMany(t=>Observable.FromAsync(async ()=>await handler(t)))
                .Catch<Unit, Exception>(ex =>
                {
                    try
                    {
                        innerExecptionHandler(ex);
                    }
                    catch
                    {
                        //intentionally blank
                    }

                    return CreateObservable();
                });
        }
        return CreateObservable().Subscribe();
    }

    private static void SilentlySwallowErrors(Exception obj)
    {
        //intentionally blank
    }
}