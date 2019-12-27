using System;
using System.Reactive.Linq;
using Library.Interface.File;
using Library.Interface.Message;
using Library.Interface.Reactive;
using Library.Reactive;
using ObservableExtensions = Library.Reactive.ObservableExtensions;

namespace Library.File
{
    public class MessageObservableFactory : IBroadcastMessageObservableFactory
    {
        private readonly IFileMessageService _fileMessageService;

        public MessageObservableFactory(IFileMessageService fileMessageService)
        {
            _fileMessageService = fileMessageService;
        }
        public IObservable<IMessages> GetFileBroadcast(string fileName, 
            IFileMessageConfig fileMessageConfig,
            IScheduler scheduler,
            Guid? broadcastId = null)
        {
            broadcastId = broadcastId ?? Guid.NewGuid();
            var payloadObservable = GetPayloads(fileName, broadcastId.Value, fileMessageConfig);

            var fileHeaderObservable = GetFileHeaderObservable(fileName, 
                fileMessageConfig,
                payloadObservable,
                broadcastId: broadcastId.Value,
                scheduler: scheduler);

            var broadcastHeaderObservable = GetBroadcastHeaderObservable(fileName, 
                fileMessageConfig,
                fileHeaderObservable,
                broadcastId: broadcastId.Value,
                scheduler: scheduler);

            var messagesObservable = broadcastHeaderObservable
                .Select(broadcastHeader=>(IMessages)new Messages(){BroadcastHeader = broadcastHeader })
                .Merge(fileHeaderObservable
                    .Select(fileHeader=>(IMessages)new Messages(){FileHeader = fileHeader }), scheduler)
                .Merge(payloadObservable
                    .Select(payload=>(IMessages)new Messages(){PayloadMessage = payload }), scheduler); 
            
            return messagesObservable;
        }

        public IObservable<IBroadcastHeader> GetBroadcastHeaderObservable<TComplete>(string fileName, 
            IFileMessageConfig fileMessageConfig,
            IObservable<TComplete> onCompletionObservable,
            Guid broadcastId,
            IScheduler scheduler)
        {
            var broadcastHeader = _fileMessageService.GetBroadcastHeader(fileName, broadcastId, fileMessageConfig);
            
            var firstObservable = (new IBroadcastHeader[] { broadcastHeader })
                .ToObservable();
            
            var lastObservable = onCompletionObservable
                .LastOrDefaultAsync()
                .Select(_=>_fileMessageService.GetBroadcastHeader(fileName, broadcastId, fileMessageConfig, true));

            var periodicObservable = ObservableExtensions
                .Timer(fileMessageConfig.HeaderRebroadcastInterval, fileMessageConfig.HeaderRebroadcastInterval, scheduler)
                .TakeUntil(lastObservable)
                .Select(l=>broadcastHeader);            

            var bcHeaderObservable = firstObservable
                .Concat(lastObservable)
                .Merge(periodicObservable);

            return bcHeaderObservable;
        }

        public IObservable<IFileHeader> GetFileHeaderObservable<TComplete>(string fileName, 
            IFileMessageConfig fileMessageConfig,
            IObservable<TComplete> onCompletionObservable,
            Guid broadcastId,
            IScheduler scheduler)
        {
            var broadcastHeader = _fileMessageService.GetFileHeader(fileName, broadcastId, fileMessageConfig, 0);
            
            var firstObservable = (new IFileHeader[] { broadcastHeader })
                .ToObservable();
            
            var lastObservable = onCompletionObservable
                .LastOrDefaultAsync()
                .Select(_=>_fileMessageService.GetFileHeader(fileName, broadcastId, fileMessageConfig, 0, true));

            var periodicObservable = ObservableExtensions
                .Timer(fileMessageConfig.HeaderRebroadcastInterval, fileMessageConfig.HeaderRebroadcastInterval, scheduler)
                .TakeUntil(lastObservable)
                .Select(l=>broadcastHeader);            

            var bcHeaderObservable = firstObservable
                .Concat(lastObservable)
                .Merge(periodicObservable);

            return bcHeaderObservable;
        }

        /// <summary>
        /// Returns an observable that represents all of the payloads from a given file
        /// </summary>
        /// <param name="fileName">the file to read</param>
        /// <param name="broadcastId">the id for the current broadcast</param>
        /// <param name="startingPayloadIndex">Optional - If this is not the first payload in the broadcast: specify at what payload index this file should start</param>
        /// <param name="startingBytes">Optional - If this is not the first payload in the broadcast: specify any bytes which should be prepended to the first payload of this series</param>
        /// <returns></returns>
        public IObservable<IPayloadMessage> GetPayloads(string fileName, 
            Guid broadcastId,
            IFileMessageConfig fileMessageConfig, 
            long startingPayloadIndex = 0,
            byte[] startingBytes = null)
        {
            return _fileMessageService.GetPayloads(fileName, broadcastId, fileMessageConfig, startingPayloadIndex: startingPayloadIndex, startingBytes: startingBytes);
        }
    }
}
