using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Caching.Memory;

namespace Library.Message
{
    public class NaivePayloadCache : IPayloadCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IMessageCacheConfig _messageCacheConfig;
        private readonly Subject<ICachedMessage> _cachedMessageSubject;

        public NaivePayloadCache(IMemoryCache memoryCache,
            IMessageCacheConfig messageCacheConfig)
        {
            _memoryCache = memoryCache;
            _messageCacheConfig = messageCacheConfig;

            
            _cachedMessageSubject = new Subject<ICachedMessage>();
            CachedObservable = _cachedMessageSubject.AsObservable();
        }

        public void Handle(IObservable<IFileReadyMessage> observable)
        {
            observable
                .Subscribe(fileReadyMessage =>
                {
                    _memoryCache.GetOrCreate(
                        new FileReadyCacheKey(fileReadyMessage.BroadcastId, fileReadyMessage.FileName),
                        cacheEntry =>
                        {
                            cacheEntry.SlidingExpiration = _messageCacheConfig.BroadcastCacheExpiration;
                            return fileReadyMessage;
                        });
                    if (_memoryCache.TryGetValue(new PayloadCacheKey(fileReadyMessage.BroadcastId, 0),
                        out object cachedObject))
                    {
                        var queue = (ConcurrentQueue<IPayloadMessage>) cachedObject;
                        while (queue.TryDequeue(out var payloadMessage))
                        {
                            _cachedMessageSubject.OnNext(new CachedMessage(fileReadyMessage.BroadcastId, 
                                payloadMessage.ChunkIndex, 
                                payloadMessage.PayloadIndex,
                                payloadMessage.Payload, 
                                fileReadyMessage.FileName,
                                null));
                        }
                    }
                });
        }

        public void Handle(IObservable<IPayloadMessage> observable)
        {
            throw new NotImplementedException();
            //observable
            //    .Subscribe(m =>
            //    {
            //        var broadcastId = m.BroadcastHeader?.BroadcastId ??
            //                          m.ChunkHeader?.BroadcastId ??
            //                          m.FileHeader?.BroadcastId ??
            //                          m.PayloadMessage.BroadcastId;
            //        var cacheValue = _memoryCache.GetOrCreate(new BroadcastCacheKey(broadcastId),
            //            cacheEntry =>
            //            {
            //                cacheEntry.SlidingExpiration = _messageCacheConfig.BroadcastCacheExpiration;
            //                return new BroadcastCacheValue(broadcastId)
            //                {
            //                    FileName = m.FileHeader.FileName
            //                };
            //            });
            //        //if (string.IsNullOrEmpty(cacheValue.FileName) &&
            //        // m.PayloadMessage != null)
            //        //{
            //        //    var payloadQueue = _memoryCache.GetOrCreate(GetPayloadQueueKey(broadcastId), cacheEntry =>
            //        //    {
            //        //        cacheEntry.SlidingExpiration = _messageCacheConfig.HeaderlessPayloadExpiration
            //        //        var newQueue = new ConcurrentQueue<IPayloadMessage>();
            //        //        return newQueue;
            //        //    });
            //        //    payloadQueue.Enqueue(m.PayloadMessage);
            //        //}

            //        if(!string.IsNullOrEmpty(cacheValue.FileName))
            //        {
            //            //if (_memoryCache.TryGetValue(GetPayloadQueueKey(broadcastId), out ConcurrentQueue<IPayloadMessage> payloadQueue))
            //            //{
            //            //    while (payloadQueue.TryDequeue(out var payloadMessage))
            //            //    {
            //            //        _cachedMessageSubject
            //            //            .OnNext(new CachedMessage(payloadMessage.BroadcastId,
            //            //                payloadMessage.ChunkIndex,
            //            //                payloadMessage.PayloadIndex,
            //            //                payloadMessage.Payload,
            //            //                cacheValue.FileName,
            //            //                BigInteger.Zero));
            //            //    } 
            //            //}
            //            if (m.PayloadMessage != null)
            //            {
            //                _cachedMessageSubject
            //                    .OnNext(new CachedMessage(m.PayloadMessage.BroadcastId,
            //                        m.PayloadMessage.ChunkIndex,
            //                        m.PayloadMessage.PayloadIndex,
            //                        m.PayloadMessage.Payload,
            //                        cacheValue.FileName,
            //                        BigInteger.Zero));
            //            }
            //        }
            //    });
        }

        public IObservable<ICachedMessage> CachedObservable { get; }

        internal class FileReadyCacheKey
        {
            public FileReadyCacheKey(Guid broadcastId, 
                string fileName)
            {
                BroadcastId = broadcastId;
                FileName = fileName;
            }

            public Guid BroadcastId { get; }
            public string FileName { get; }
            public override bool Equals(object obj)
            {
                var other = obj as FileReadyCacheKey;
                if (other == null) return false;
                return BroadcastId.Equals(other.BroadcastId) && string.Equals(FileName, other.FileName);
            }

            public override int GetHashCode()
            {
                const int magicHash1 = 17;
                const int magicHash2 = 31;
                int hash = magicHash1;
                hash = hash * magicHash2 + BroadcastId.GetHashCode();
                hash = hash * magicHash2 + (FileName?.GetHashCode() ?? default);
                return hash;
            }
        }

        internal class PayloadCacheKey
        {
            public PayloadCacheKey(Guid broadcastId, 
                BigInteger chunkIndex)
            {
                BroadcastId = broadcastId;
                ChunkIndex = chunkIndex;
            }

            public Guid BroadcastId { get; }
            public BigInteger ChunkIndex { get; }
            public override bool Equals(object obj)
            {
                var other = obj as PayloadCacheKey;
                if (other == null) return false;
                return BroadcastId.Equals(other.BroadcastId) && ChunkIndex.Equals(other.ChunkIndex);
            }

            public override int GetHashCode()
            {
                const int magicHash1 = 17;
                const int magicHash2 = 31;
                int hash = magicHash1;
                hash = hash * magicHash2 + BroadcastId.GetHashCode();
                hash = hash * magicHash2 + ChunkIndex.GetHashCode();
                return hash;
            }
        }
    }
}