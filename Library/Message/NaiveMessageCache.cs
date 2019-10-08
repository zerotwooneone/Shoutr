using System;
using Library.Listen;

namespace Library.Message
{
    public class NaiveMessageCache : IMessageCache
    {
        private readonly IHeaderCache _headerCache;
        private readonly IPayloadCache _payloadCache;

        public NaiveMessageCache(IHeaderCache headerCache,
            IPayloadCache payloadCache)
        {
            _headerCache = headerCache;
            _payloadCache = payloadCache;
        }
        public void Handle(IObservable<IReceivedMessage> messagesObservable)
        {
            _headerCache.HandleBroadcastHeader(messagesObservable.GetHeaderObservable());
            _headerCache.HandleChunkHeader(messagesObservable.GetChunkObservable());
            _headerCache.HandleFileHeader(messagesObservable.GetFileObservable());

            _payloadCache.HandlePayload(messagesObservable.GetPayloadObservable());
            _payloadCache.HandleFileReady(_headerCache.FileReadyObservable);
        }

        public IObservable<ICachedMessage> CachedObservable => _payloadCache.CachedObservable;
    }
}