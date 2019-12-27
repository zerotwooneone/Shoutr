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
using Microsoft.Extensions.Options;
using Xunit;
using System.Numerics;
using Library.Interface.Message;

namespace Library.Tests.Message
{
    public class NaivePayloadCacheTests : IDisposable
    {
        private MockRepository mockRepository;

        private MemoryCache _memoryCache;
        private Mock<IMessageCacheConfig> mockMessageCacheConfig;
        private readonly Mock<Func<Guid, BigInteger, BigInteger, string>> mockGetFileName;

        public NaivePayloadCacheTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            _memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            this.mockMessageCacheConfig = this.mockRepository.Create<IMessageCacheConfig>();            
            this.mockGetFileName = this.mockRepository.Create<Func<Guid, BigInteger, BigInteger, string>>();
        }

        public void Dispose()
        {
            mockRepository.VerifyAll();
        }

        private NaivePayloadCache CreateNaivePayloadCache()
        {
            return new NaivePayloadCache(
                _memoryCache,
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
            
            _memoryCache
                .Set(new NaivePayloadCache.PayloadCacheKey(broadcastId, 0), payloadMessages);

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

            _memoryCache
                .Set(new NaivePayloadCache.PayloadCacheKey(broadcastId, 0), payloadMessages);

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

            _memoryCache
                .Set(new NaivePayloadCache.FileReadyCacheKey(broadcastId, "file name"), 
                    new FileReadyMessage(broadcastId, "file name", 0));

            mockGetFileName
                .Setup(gf=>gf(It.IsAny<Guid>(), It.IsAny<BigInteger>(), It.IsAny<BigInteger>()))
                .Returns("file name");
            
            var actual = naivePayloadCache
                .CachedObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            naivePayloadCache.HandlePayload(
                observable,
                mockGetFileName.Object);

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
            
            mockMessageCacheConfig
                .SetupGet(mcc => mcc.ChunkPayloadCacheExpiration)
                .Returns(TimeSpan.MaxValue);

            mockGetFileName
                .Setup(gf=>gf(It.IsAny<Guid>(), It.IsAny<BigInteger>(), It.IsAny<BigInteger>()))
                .Returns("file name");
            
            // Act
            naivePayloadCache.HandlePayload(
                observable,
                mockGetFileName.Object);
               
            // Assert
            var actual = _memoryCache
                .Get<ConcurrentQueue<IPayloadMessage>>(new NaivePayloadCache.PayloadCacheKey(broadcastId, 0))
                .First();
            Assert.Equal(expected, actual.Payload);
        }
    }
}
