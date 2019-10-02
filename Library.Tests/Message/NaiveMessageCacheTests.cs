using Library.Message;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace Library.Tests.Message
{
    public class NaiveMessageCacheTests : IDisposable
    {
        private MockRepository mockRepository;

        private Mock<IMemoryCache> mockMemoryCache;
        private Mock<IReceivedMessage> mockMessage;
        private Mock<IBroadcastHeader> mockBroadcastHeader;
        private Mock<ICacheEntry> mockBroadcastCacheEntry;
        private Mock<ICacheEntry> mockPayloadCacheEntry;
        private Mock<IMessageCacheConfig> messageCacheConfig;
        private Mock<IPayloadMessage> mockPayloadMessage;
        private Mock<IChunkHeader> mockChunkHeader;
        private Mock<IFileHeader> mockFileHeader;

        public NaiveMessageCacheTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockMemoryCache = this.mockRepository.Create<IMemoryCache>();
            mockMessage = mockRepository.Create<IReceivedMessage>();
            mockBroadcastHeader = mockRepository.Create<IBroadcastHeader>();
            mockBroadcastCacheEntry = mockRepository.Create<ICacheEntry>();
            mockPayloadCacheEntry = mockRepository.Create<ICacheEntry>();
            messageCacheConfig = mockRepository.Create<IMessageCacheConfig>();
            mockPayloadMessage = mockRepository.Create<IPayloadMessage>();
            mockChunkHeader = mockRepository.Create<IChunkHeader>();
            mockFileHeader = mockRepository.Create<IFileHeader>();
        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        private NaiveMessageCache CreateNaiveHeaderCache()
        {
            return new NaiveMessageCache(
                this.mockMemoryCache.Object,
                messageCacheConfig.Object);
        }

        [Fact]
        public void Handle_BroadcastHeaderWhenColdCache_Caches()
        {
            // Arrange
            var NaiveHeaderCache = this.CreateNaiveHeaderCache();

            var broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            SetupBroadcastHeaderMessage(broadcastId);
            IObservable<IReceivedMessage> messagesObservable = new BehaviorSubject<IReceivedMessage>(mockMessage.Object);
            SetupGetOrCreateCache();
            messageCacheConfig
                .SetupGet(mcc => mcc.BroadcastCacheExpiration)
                .Returns(TimeSpan.MaxValue);
            mockBroadcastCacheEntry
                .SetupSet(mce=>mce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();
            //mockMessage
            //    .SetupGet(mm => mm.PayloadMessage)
            //    .Returns((IPayloadMessage)null);

            // Act
            NaiveHeaderCache.Handle(
                messagesObservable);

            // Assert
            mockMemoryCache
                .Verify(mmc => mmc.CreateEntry(It.Is<BroadcastCacheKey>(bck=>bck.BroadCastId == broadcastId)), 
                    Times.Once);
        }
        
        [Fact]
        public void Handle_BroadcastHeaderWhenColdCache_SetSlidingExpiration()
        {
            // Arrange
            var NaiveHeaderCache = this.CreateNaiveHeaderCache();

            var broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            SetupBroadcastHeaderMessage(broadcastId);
            IObservable<IReceivedMessage> messagesObservable = new BehaviorSubject<IReceivedMessage>(mockMessage.Object);
            SetupGetOrCreateCache();
            messageCacheConfig
                .SetupGet(mcc => mcc.BroadcastCacheExpiration)
                .Returns(TimeSpan.MaxValue);
            mockBroadcastCacheEntry
                .SetupSet(mce=>mce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();
            //mockMessage
            //    .SetupGet(mm => mm.PayloadMessage)
            //    .Returns((IPayloadMessage)null);

            // Act
            NaiveHeaderCache.Handle(
                messagesObservable);

            // Assert
            mockBroadcastCacheEntry
                .VerifySet(mce=>mce.SlidingExpiration = It.Is<TimeSpan?>(ts=>messageCacheConfig.Object.BroadcastCacheExpiration.Equals(ts)), 
                    Times.Once);
        }

        [Fact]
        public void Handle_BroadcastHeaderWhenCached_DoesNotCache()
        {
            // Arrange
            var NaiveHeaderCache = this.CreateNaiveHeaderCache();

            var broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            mockBroadcastHeader
                .SetupGet(mh => mh.BroadcastId)
                .Returns(broadcastId);
            mockMessage
                .SetupGet(mm => mm.BroadcastHeader)
                .Returns(mockBroadcastHeader.Object);
            //mockMessage
            //    .SetupGet(mm => mm.PayloadMessage)
            //    .Returns((IPayloadMessage)null);
            IObservable<IReceivedMessage> messagesObservable = new BehaviorSubject<IReceivedMessage>(mockMessage.Object);
            object allreadyCached = new BroadcastCacheValue(broadcastId);
            mockMemoryCache
                .Setup(mmc => mmc.TryGetValue(It.IsAny<object>(), out allreadyCached))
                .Returns(allreadyCached != null);

            // Act
            NaiveHeaderCache.Handle(
                messagesObservable);

            // Assert
            mockMemoryCache
                .Verify(mmc => mmc.CreateEntry(It.IsAny<object>()), 
                    Times.Never);
        }

        [Fact]
        public async Task Handle_PayloadMessageWhenCached_EmitsFilename()
        {
            // Arrange
            var NaiveHeaderCache = this.CreateNaiveHeaderCache();

            var broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            mockPayloadMessage
                .SetupGet(mpm => mpm.BroadcastId)
                .Returns(broadcastId);
            mockPayloadMessage
                .SetupGet(mpm => mpm.Payload)
                .Returns(new byte[]{16,16,99});
            mockPayloadMessage
                .SetupGet(mpm => mpm.PayloadIndex)
                .Returns(0);
            mockPayloadMessage
                .SetupGet(mpm => mpm.ChunkIndex)
                .Returns(0);
            mockMessage
                .SetupGet(mm => mm.PayloadMessage)
                .Returns(mockPayloadMessage.Object);
            mockMessage
                .SetupGet(mm => mm.BroadcastHeader)
                .Returns((IBroadcastHeader)null);
            mockMessage
                .SetupGet(mm => mm.ChunkHeader)
                .Returns((IChunkHeader)null);
            mockMessage
                .SetupGet(mm => mm.FileHeader)
                .Returns((IFileHeader)null);
            IObservable<IReceivedMessage> messagesObservable = new BehaviorSubject<IReceivedMessage>(mockMessage.Object);
            var expected = "Some Filename";
            object allreadyCached = new BroadcastCacheValue(broadcastId)
            {
                FileName = expected
            };
            mockMemoryCache
                .Setup(mmc => mmc.TryGetValue(It.IsAny<object>(), out allreadyCached))
                .Returns(allreadyCached != null);
            var task = NaiveHeaderCache
                .CachedObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            NaiveHeaderCache.Handle(
                messagesObservable);
            var actual = await task;

            // Assert
            Assert.Equal(expected, actual.FileName);
        }

        [Fact]
        public async Task Handle_PayloadMessageWhenCached_EmitsBytes()
        {
            // Arrange
            var NaiveHeaderCache = this.CreateNaiveHeaderCache();

            var broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            mockPayloadMessage
                .SetupGet(mpm => mpm.BroadcastId)
                .Returns(broadcastId);
            var expected = new byte[]{16,16,99};
            mockPayloadMessage
                .SetupGet(mpm => mpm.Payload)
                .Returns(expected);
            mockPayloadMessage
                .SetupGet(mpm => mpm.PayloadIndex)
                .Returns(0);
            mockPayloadMessage
                .SetupGet(mpm => mpm.ChunkIndex)
                .Returns(0);
            mockMessage
                .SetupGet(mm => mm.PayloadMessage)
                .Returns(mockPayloadMessage.Object);
            mockMessage
                .SetupGet(mm => mm.BroadcastHeader)
                .Returns((IBroadcastHeader)null);
            mockMessage
                .SetupGet(mm => mm.ChunkHeader)
                .Returns((IChunkHeader)null);
            mockMessage
                .SetupGet(mm => mm.FileHeader)
                .Returns((IFileHeader)null);
            IObservable<IReceivedMessage> messagesObservable = new BehaviorSubject<IReceivedMessage>(mockMessage.Object);
            object allreadyCached = new BroadcastCacheValue(broadcastId)
            {
                FileName = "Some Filename"
            };
            mockMemoryCache
                .Setup(mmc => mmc.TryGetValue(It.IsAny<object>(), out allreadyCached))
                .Returns(allreadyCached != null);
            var task = NaiveHeaderCache
                .CachedObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            NaiveHeaderCache.Handle(
                messagesObservable);
            var actual = await task;

            // Assert
            Assert.Equal(expected, actual.Payload);
        }

        [Fact]
        public void Handle_PayloadMessageWhenColdCache_Caches()
        {
            // Arrange
            var NaiveHeaderCache = this.CreateNaiveHeaderCache();

            var broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            SetupPayloadMessage(broadcastId,
                new byte[]{16,16,99},
                0,
                0);
            IObservable<IReceivedMessage> messagesObservable = new BehaviorSubject<IReceivedMessage>(mockMessage.Object);
            
            object emptyCache = null;
            mockMemoryCache
                .Setup(mmc => mmc.TryGetValue(It.IsAny<BroadcastCacheKey>(), out emptyCache))
                .Returns(false);
            mockMemoryCache
                .Setup(mmc => mmc.CreateEntry(It.IsAny<BroadcastCacheKey>()))
                .Returns(mockBroadcastCacheEntry.Object);
            
            mockMemoryCache
                .Setup(mmc => mmc.TryGetValue(It.IsAny<PayloadQueueCacheKey>(), out emptyCache))
                .Returns(false);
            mockMemoryCache
                .Setup(mmc => mmc.CreateEntry(It.IsAny<PayloadQueueCacheKey>()))
                .Returns(mockPayloadCacheEntry.Object);

            mockPayloadCacheEntry
                .SetupSet(mce => mce.Value =It.IsAny<PayloadQueueCacheKey>());
            mockPayloadCacheEntry
                .Setup(mce=>mce.Dispose())
                .Verifiable();

            mockBroadcastCacheEntry
                .SetupSet(mce => mce.Value =It.IsAny<BroadcastCacheKey>());
            mockBroadcastCacheEntry
                .Setup(mce=>mce.Dispose())
                .Verifiable();

            messageCacheConfig
                .SetupGet(mcc => mcc.ChunkPayloadCacheExpiration)
                .Returns(TimeSpan.MaxValue);
            mockBroadcastCacheEntry
                .SetupSet(mce=>mce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();
            mockPayloadCacheEntry
                .SetupSet(mce=>mce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();

            // Act
            NaiveHeaderCache.Handle(
                messagesObservable);

            // Assert
            mockMemoryCache
                .Verify(mmc => mmc.CreateEntry(It.Is<PayloadQueueCacheKey>(bck=>bck.BroadCastId == broadcastId)), 
                    Times.Once);
        }

        private void SetupMockCacheEntry()
        {
            mockBroadcastCacheEntry
                .SetupSet(mce => mce.Value =It.IsAny<object>());
            mockBroadcastCacheEntry
                .Setup(mce=>mce.Dispose())
                .Verifiable();
        }

        private void SetupGetOrCreateCache()
        {
            object allreadyCached = null;
            mockMemoryCache
                .Setup(mmc => mmc.TryGetValue(It.IsAny<object>(), out allreadyCached))
                .Returns(allreadyCached != null);
            mockMemoryCache
                .Setup(mmc => mmc.CreateEntry(It.IsAny<object>()))
                .Returns(mockBroadcastCacheEntry.Object);
            SetupMockCacheEntry();
        }

        private void SetupBroadcastHeaderMessage(Guid broadcastId)
        {
            mockBroadcastHeader
                .SetupGet(mh => mh.BroadcastId)
                .Returns(broadcastId);
            mockMessage
                .SetupGet(mm => mm.BroadcastHeader)
                .Returns(mockBroadcastHeader.Object);
            mockMessage
                .SetupGet(mm => mm.FileHeader)
                .Returns((IFileHeader)null);
        }

        private void SetupPayloadMessage(Guid broadcastId,
            byte[] payload,
            BigInteger? payloadIndex = null,
            BigInteger? chunkIndex = null)
        {
            mockPayloadMessage
                .SetupGet(mpm => mpm.BroadcastId)
                .Returns(broadcastId);
            mockPayloadMessage
                .SetupGet(mpm => mpm.Payload)
                .Returns(payload);
            mockPayloadMessage
                .When(()=>payloadIndex !=null)
                .SetupGet(mpm => mpm.PayloadIndex)
                .Returns(payloadIndex.Value);
            mockPayloadMessage
                .When(()=>chunkIndex !=null)
                .SetupGet(mpm => mpm.ChunkIndex)
                .Returns(chunkIndex.Value);

            mockMessage
                .SetupGet(mm => mm.BroadcastHeader)
                .Returns((IBroadcastHeader)null);
            mockMessage
                .SetupGet(mm => mm.ChunkHeader)
                .Returns((IChunkHeader)null);
            mockMessage
                .SetupGet(mm => mm.FileHeader)
                .Returns((IFileHeader)null);
        }
    }
}
