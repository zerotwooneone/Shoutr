using Library.Message;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Library.Tests.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Library.Tests.Message
{
    public class NaivePayloadCacheTests : IDisposable
    {
        private MockRepository mockRepository;

        private Mock<IMemoryCache> mockMemoryCache;
        private Mock<IMessageCacheConfig> mockMessageCacheConfig;
        private Mock<ICacheEntry> _payloadCacheEntry;
        private Mock<ICacheEntry> _fileReadyCacheEntry;

        public NaivePayloadCacheTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockMemoryCache = mockRepository.Create<IMemoryCache>();
            this.mockMessageCacheConfig = this.mockRepository.Create<IMessageCacheConfig>();
            _payloadCacheEntry = mockRepository.Create<ICacheEntry>();
            _fileReadyCacheEntry = mockRepository.Create<ICacheEntry>();
        }

        public void Dispose()
        {
            //mockRepository.VerifyAll();
        }

        private NaivePayloadCache CreateNaivePayloadCache()
        {
            return new NaivePayloadCache(
                this.mockMemoryCache.Object,
                this.mockMessageCacheConfig.Object);
        }

        [Fact]
        public async Task HandleFileReady_WhenPayloadCached_EmitsPayload()
        {
            // Arrange
            var naivePayloadCache = this.CreateNaivePayloadCache();
            Guid broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            var observable = new BehaviorSubject<IFileReadyMessage>(new FileReadyMessage(broadcastId, "file name", 0));

            var payloadMessages = new ConcurrentQueue<IPayloadMessage>();
            var expected = new byte[] {16, 16, 99};
            payloadMessages.Enqueue(new PayloadMessage(broadcastId, 0, expected, 0));

            _fileReadyCacheEntry
                .SetupSet(pce => pce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();

            mockMemoryCache
                .SetupGetOrCreate<NaivePayloadCache.FileReadyCacheKey, FileReadyMessage>(_fileReadyCacheEntry);

            var existingObject = (object) payloadMessages;
            mockMemoryCache
                .Setup(mc => mc.TryGetValue((object) It.IsAny<NaivePayloadCache.PayloadCacheKey>(), out existingObject))
                .Returns(true);

            mockMessageCacheConfig
                .SetupGet(mcc => mcc.BroadcastCacheExpiration)
                .Returns(TimeSpan.MaxValue);

            var actual = naivePayloadCache
                .CachedObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            naivePayloadCache.HandleFileReady(
                observable);

            // Assert
            Assert.Equal(expected, (await actual).Payload);
        }

        [Fact]
        public async Task HandleFileReady_WhenPayloadCached_EmitsFilename()
        {
            // Arrange
            var naivePayloadCache = this.CreateNaivePayloadCache();
            Guid broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            var expected = "file name";
            var observable = new BehaviorSubject<IFileReadyMessage>(new FileReadyMessage(broadcastId, expected, 0));

            var payloadMessages = new ConcurrentQueue<IPayloadMessage>();
            payloadMessages.Enqueue(new PayloadMessage(broadcastId, 0, new byte[]{16,16,99}, 0));
            
            _fileReadyCacheEntry
                .SetupSet(pce=>pce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();

            mockMemoryCache
                .SetupGetOrCreate<NaivePayloadCache.FileReadyCacheKey, FileReadyMessage>(_fileReadyCacheEntry);

            var existingObject = (object) payloadMessages;
            mockMemoryCache
                .Setup(mc => mc.TryGetValue((object)It.IsAny<NaivePayloadCache.PayloadCacheKey>(), out existingObject)) 
                .Returns(true);

            mockMessageCacheConfig
                .SetupGet(mcc => mcc.BroadcastCacheExpiration)
                .Returns(TimeSpan.MaxValue);

            var actual = naivePayloadCache
                .CachedObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            naivePayloadCache.HandleFileReady(
                observable);

            // Assert
            Assert.Equal(expected, (await actual).FileName);
        }

        [Fact]
        public async Task HandlePayload_WhenFileReady_EmitsPayload()
        {
            // Arrange
            var naivePayloadCache = this.CreateNaivePayloadCache();
            Guid broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            var expected = new byte[]{16,16,99};
            var observable = new BehaviorSubject<IPayloadMessage>(new PayloadMessage(broadcastId, 0, expected, 0));

            var fileReadyObject = (object) new FileReadyMessage(broadcastId, "file name", 0);
            mockMemoryCache
                .Setup(mc => mc.TryGetValue((object)It.IsAny<NaivePayloadCache.FileReadyCacheKey>(), out fileReadyObject)) 
                .Returns(true);
            
            var actual = naivePayloadCache
                .CachedObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            naivePayloadCache.HandlePayload(
                observable);

            // Assert
            Assert.Equal(expected, (await actual).Payload);
        }

        [Fact]
        public async Task HandlePayload_WhenCacheCold_CachePayload()
        {
            // Arrange
            var naivePayloadCache = this.CreateNaivePayloadCache();
            Guid broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            var expected = new byte[]{16,16,99};
            var observable = new BehaviorSubject<IPayloadMessage>(new PayloadMessage(broadcastId, 0, expected, 0));

            var fileReadyObject = (object) null;
            mockMemoryCache
                .Setup(mc => mc.TryGetValue((object)It.IsAny<NaivePayloadCache.FileReadyCacheKey>(), out fileReadyObject)) 
                .Returns(false);

            mockMemoryCache
                .SetupGetOrCreate<NaivePayloadCache.PayloadCacheKey, ConcurrentQueue<IPayloadMessage>>(_payloadCacheEntry);

            mockMessageCacheConfig
                .SetupGet(mcc => mcc.ChunkPayloadCacheExpiration)
                .Returns(TimeSpan.MaxValue);

            _payloadCacheEntry
                .SetupSet(pce => pce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();

            //this is a dirty hack to get access to the queue. Improve this with dependency injection
            ConcurrentQueue<IPayloadMessage> queue = null;
            _payloadCacheEntry
                .SetupSet(pce=>pce.Value = It.IsAny<ConcurrentQueue<IPayloadMessage>>())
                .Callback((object q)=>queue = (ConcurrentQueue<IPayloadMessage>)q);

            // Act
            naivePayloadCache.HandlePayload(
                observable);

            // Assert
            var actual = queue.First();
            Assert.Equal(expected, actual.Payload);
        }
    }
}
