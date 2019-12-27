using Library.Reactive;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Library.Interface.Broadcast;
using Library.Interface.Configuration;
using Library.Interface.File;
using Library.Interface.Reactive;

namespace Library.Broadcast
{
    public class NaiveBroadcastService : IBroadcastService
    {
        private readonly IBroadcastMessageObservableFactory _broadcastMessageObservableFactory;
        private readonly IConfigurationService configurationService;

        public NaiveBroadcastService(IBroadcastMessageObservableFactory broadcastMessageObservableFactory,
            IConfigurationService configurationService)
        {
            this._broadcastMessageObservableFactory = broadcastMessageObservableFactory;
            this.configurationService = configurationService;
        }

        public async Task BroadcastFile(string fileName, 
            IFileMessageConfig fileMessageConfig, 
            IBroadcaster broadcaster, 
            IScheduler scheduler)
        {
            var broadcastId = Guid.NewGuid();
            
            var messagesObservable = _broadcastMessageObservableFactory.GetFileBroadcast(fileName, fileMessageConfig, scheduler, broadcastId);

            var throttled = messagesObservable.TickingThrottle(configurationService.TimeBetweenBroadcasts.Value, scheduler);

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
}