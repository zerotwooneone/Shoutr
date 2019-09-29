using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Caching.Memory;

namespace Library.Message
{
    public class NaiveHeaderCache : IHeaderCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IMessageCacheConfig _messageCacheConfig;
        private readonly Subject<ICachedMessage> _cachedMessageSubject;

        public NaiveHeaderCache(IMemoryCache memoryCache,
            IMessageCacheConfig messageCacheConfig)
        {
            _memoryCache = memoryCache;
            _messageCacheConfig = messageCacheConfig;

            
            _cachedMessageSubject = new Subject<ICachedMessage>();
            CachedObservable = _cachedMessageSubject.AsObservable();
        }
        public void Handle(IObservable<IReceivedMessage> messagesObservable)
        {
            messagesObservable
                .Subscribe(m =>
                {
                    var broadcastId = m.BroadcastHeader?.BroadcastId ??
                                      m.ChunkHeader?.BroadcastId ??
                                      m.FileHeader?.BroadcastId ??
                                      m.PayloadMessage.BroadcastId;
                    var cacheValue = _memoryCache.GetOrCreate(new BroadcastCacheKey(broadcastId),
                        cacheEntry =>
                        {
                            cacheEntry.SlidingExpiration = _messageCacheConfig.BroadcastCacheExpiration;
                            return new BroadcastCacheValue(broadcastId)
                            {
                                FileName = m.FileHeader?.FileName
                            };
                        });
                    //if (string.IsNullOrEmpty(cacheValue.FileName) &&
                    // m.PayloadMessage != null)
                    //{
                    //    var payloadQueue = _memoryCache.GetOrCreate(GetPayloadQueueKey(broadcastId), cacheEntry =>
                    //    {
                    //        cacheEntry.SlidingExpiration = _messageCacheConfig.HeaderlessPayloadExpiration
                    //        var newQueue = new ConcurrentQueue<IPayloadMessage>();
                    //        return newQueue;
                    //    });
                    //    payloadQueue.Enqueue(m.PayloadMessage);
                    //}

                    if(!string.IsNullOrEmpty(cacheValue.FileName))
                    {
                        //if (_memoryCache.TryGetValue(GetPayloadQueueKey(broadcastId), out ConcurrentQueue<IPayloadMessage> payloadQueue))
                        //{
                        //    while (payloadQueue.TryDequeue(out var payloadMessage))
                        //    {
                        //        _cachedMessageSubject
                        //            .OnNext(new CachedMessage(payloadMessage.BroadcastId,
                        //                payloadMessage.ChunkIndex,
                        //                payloadMessage.PayloadIndex,
                        //                payloadMessage.Payload,
                        //                cacheValue.FileName,
                        //                BigInteger.Zero));
                        //    } 
                        //}
                        if (m.PayloadMessage != null)
                        {
                            _cachedMessageSubject
                                .OnNext(new CachedMessage(m.PayloadMessage.BroadcastId,
                                    m.PayloadMessage.ChunkIndex,
                                    m.PayloadMessage.PayloadIndex,
                                    m.PayloadMessage.Payload,
                                    cacheValue.FileName,
                                    BigInteger.Zero));
                        }
                    }
                });
        }

        private static string GetPayloadQueueKey(Guid broadcastId)
        {
            return $"{broadcastId} Payloads";
        }

        public IObservable<ICachedMessage> CachedObservable { get; }
    }
}