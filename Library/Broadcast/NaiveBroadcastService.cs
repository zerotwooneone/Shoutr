using Library.File;
using Library.Reactive;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Library.Broadcast
{
    public class NaiveBroadcastService : IBroadcastService
    {
        private readonly IBroadcastMessageObservableFactory _broadcastMessageObservableFactory;

        public NaiveBroadcastService(IBroadcastMessageObservableFactory broadcastMessageObservableFactory)
        {
            this._broadcastMessageObservableFactory = broadcastMessageObservableFactory;
        }

        public async Task BroadcastFile(string fileName, 
            IFileMessageConfig fileMessageConfig, 
            IBroadcaster broadcaster, 
            IScheduler scheduler)
        {
            var broadcastId = Guid.NewGuid();
            
            var messagesObservable = _broadcastMessageObservableFactory.GetFileBroadcast(fileName, fileMessageConfig, broadcastId, scheduler);

            var throttled = messagesObservable.RealThrottle(TimeSpan.FromSeconds(1), scheduler);

            var broadcastObservable = throttled
                .Select(messages =>
                {
                    if(messages.BroadcastHeader != null)
                    {
                        return Observable.FromAsync(() =>
                        {
                            return broadcaster.Broadcast(messages.BroadcastHeader);
                        });
                    }else if(messages.FileHeader != null)
                    {
                        return Observable.FromAsync(()=>broadcaster.Broadcast(messages.FileHeader));
                    }else if(messages.PayloadMessage != null)
                    {
                        return Observable.FromAsync(()=>broadcaster.Broadcast(messages.PayloadMessage));
                    }
                    else
                    {
                        return Observable.Empty<System.Reactive.Unit>();
                    }
                })              
                .Concat();

            await broadcastObservable;
        }
    }

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
        public static IObservable<T> RealThrottle<T>(this IObservable<T> source, 
            TimeSpan timeBetweenEmits,
            IScheduler scheduler)
        {
            var interval = Observable.Interval(timeBetweenEmits,scheduler);
            
            return source
                .Zip(interval, (t,intervalValue)=>t);
        }
    }
}