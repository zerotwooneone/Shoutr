using Library.Message;
using Library.Reactive;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Library.File
{
    public class MessageObservableFactory
    {
        private readonly IFileMessageService _fileMessageService;
        private readonly ISchedulerProvider _schedulerProvider;

        public MessageObservableFactory(IFileMessageService fileMessageService,
            ISchedulerProvider schedulerProvider)
        {
            _fileMessageService = fileMessageService;
            _schedulerProvider = schedulerProvider;
        }
        public IObservable<IMessages> GetFile(string fileName, 
            IFileMessageConfig fileMessageConfig,
            Guid? broadcastId = null,
            IScheduler scheduler = null)
        {
            throw new NotImplementedException();            
        }

        public IObservable<IBroadcastHeader> GetBroadcastHeaderObservable<TComplete>(string fileName, 
            IFileMessageConfig fileMessageConfig,
            IObservable<TComplete> onCompletionObservable,
            Guid? broadcastId = null,
            IScheduler scheduler = null)
        {
            var bcId = broadcastId ?? Guid.NewGuid();
            var aScheduler = scheduler ?? _schedulerProvider.Default;
            var broadcastHeader = _fileMessageService.GetBroadcastHeader(fileName, bcId, fileMessageConfig);
            
            var firstObservable = (new IBroadcastHeader[] { broadcastHeader })
                .ToObservable();
            
            var lastObservable = onCompletionObservable
                .LastOrDefaultAsync()
                .Select(_=>_fileMessageService.GetBroadcastHeader(fileName, bcId, fileMessageConfig, true));

            var periodicObservable = Observable
                .Timer(fileMessageConfig.HeaderRebroadcastInterval, fileMessageConfig.HeaderRebroadcastInterval, aScheduler)
                .TakeUntil(lastObservable)
                .Select(l=>broadcastHeader);            

            var bcHeaderObservable = firstObservable
                .Concat(lastObservable)
                .Merge(periodicObservable);

            return bcHeaderObservable;
        }
    }
}
