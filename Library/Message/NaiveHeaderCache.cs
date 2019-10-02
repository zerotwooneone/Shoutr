using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Caching.Memory;

namespace Library.Message
{
    public class NaiveHeaderCache : IHeaderCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IMessageCacheConfig _messageCacheConfig;
        private readonly Subject<IFileReadyMessage> _fileReadySubject;

        public NaiveHeaderCache(IMemoryCache memoryCache,
            IMessageCacheConfig messageCacheConfig)
        {
            _memoryCache = memoryCache;
            _messageCacheConfig = messageCacheConfig;

            
            _fileReadySubject = new Subject<IFileReadyMessage>();
            FileReadyObservable = _fileReadySubject.AsObservable();
        }

        public void Handle(IObservable<IBroadcastHeader> observable)
        {
            //observable
            //    .Subscribe(m =>
            //    {
            //        var broadcastId = m.BroadcastId;
            //        var cacheValue = _memoryCache.GetOrCreate(new HeaderCacheKey(broadcastId),
            //            cacheEntry =>
            //            {
            //                cacheEntry.SlidingExpiration = _messageCacheConfig.BroadcastCacheExpiration;
            //                return new HeaderCacheValue(broadcastId);
            //            });
            //    });
            throw new NotImplementedException();
        }

        public void Handle(IObservable<IChunkHeader> observable)
        {
            throw new NotImplementedException();
        }

        public void Handle(IObservable<IFileHeader> observable)
        {
            observable
                .Subscribe(m =>
                {
                    var broadcastId = m.BroadcastId;
                    var cacheValue = _memoryCache.GetOrCreate(new HeaderCacheKey(broadcastId),
                        cacheEntry =>
                        {
                            cacheEntry.SlidingExpiration = _messageCacheConfig.BroadcastCacheExpiration;
                            return new HeaderCacheValue(broadcastId)
                            {
                                FileName = m.FileName
                            };
                        });
                    _fileReadySubject
                        .OnNext(new FileReadyMessage(broadcastId,
                            m.FileName,
                            null));
                });
        }


        public IObservable<IFileReadyMessage> FileReadyObservable { get; }

        internal class HeaderCacheKey
        {
            public HeaderCacheKey(Guid broadCastId)
            {
                BroadCastId = broadCastId;
            }

            public Guid BroadCastId { get; }

            public override bool Equals(object other)
            {
                var x = other as HeaderCacheKey;
                if (x == null) return false;
                return x.BroadCastId == BroadCastId;
            }

            public override int GetHashCode()
            {
                return BroadCastId.GetHashCode();
            }
        }

        internal class HeaderCacheValue
        {
            public HeaderCacheValue(Guid broadCastId)
            {
                BroadCastId = broadCastId;
            }

            public Guid BroadCastId { get; }
            public string FileName { get; set; }
        }
    }
}