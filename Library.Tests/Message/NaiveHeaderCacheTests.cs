using Library.Message;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Library.Tests.Extensions.Caching.Memory;
using Xunit;
using IFileHeader = Library.Message.IFileHeader;
using static Library.Message.NaiveHeaderCache;

namespace Library.Tests.Message
{
    public class NaiveHeaderCacheTests : IDisposable
    {
        private MockRepository mockRepository;

        private Mock<IMemoryCache> mockMemoryCache;
        private Mock<IMessageCacheConfig> mockMessageCacheConfig;
        private Mock<IBroadcastHeader> mockBroadcastHeader;
        private Mock<ICacheEntry> mockCacheEntry;
        private Mock<IChunkHeader> mockChunkHeader;
        private Mock<IFileHeader> mockFileHeader;

        public NaiveHeaderCacheTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockMemoryCache = this.mockRepository.Create<IMemoryCache>();
            this.mockMessageCacheConfig = this.mockRepository.Create<IMessageCacheConfig>();
            mockBroadcastHeader = mockRepository.Create<IBroadcastHeader>();
            mockCacheEntry = mockRepository.Create<ICacheEntry>();
            mockChunkHeader = mockRepository.Create<IChunkHeader>();
            mockFileHeader = mockRepository.Create<IFileHeader>();
        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        private NaiveHeaderCache CreateNaiveHeaderCache()
        {
            return new NaiveHeaderCache(
                this.mockMemoryCache.Object,
                this.mockMessageCacheConfig.Object);
        }

        [Fact]
        public void HandleBroadcastHeader_WhenCacheCold_DoesNotEmitFileReady()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var observable = new BehaviorSubject<IBroadcastHeader>(mockBroadcastHeader.Object);

            mockBroadcastHeader
                .SetupGet(fh => fh.ChunkSizeInBytes)
                .Returns(999);
            mockBroadcastHeader
                .SetupGet(fh => fh.BroadcastId)
                .Returns(Guid.Empty);

            mockMemoryCache
                .SetupGetOrCreate<HeaderCacheKey, HeaderCacheValue>(mockCacheEntry);

            mockCacheEntry
                .SetupSet(ce=>ce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();
            
            mockMessageCacheConfig
                .SetupGet(mcc => mcc.BroadcastCacheExpiration)
                .Returns(TimeSpan.MaxValue);

            bool actual = false; 
            naiveHeaderCache
                .FileReadyObservable
                .Subscribe(fr=>actual = true);

            // Act
            naiveHeaderCache.HandleBroadcastHeader(
                    observable);
            
            // Assert
            Assert.False(actual);
        }

        [Fact]
        public async Task HandleBroadcastHeader_WhenCached_EmitsFilenameReady()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var observable = new BehaviorSubject<IBroadcastHeader>(mockBroadcastHeader.Object);

            const long expected  = 999;
            mockBroadcastHeader
                .SetupGet(fh => fh.ChunkSizeInBytes)
                .Returns(expected);
            mockBroadcastHeader
                .SetupGet(fh => fh.BroadcastId)
                .Returns(Guid.Empty);

            mockMemoryCache
                .SetupGetOrCreate<HeaderCacheKey, HeaderCacheValue>(mockCacheEntry, new HeaderCacheValue(Guid.Empty){FileName = "file name" });
            
            var actual = naiveHeaderCache
                .FileReadyObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            naiveHeaderCache.HandleBroadcastHeader(
                    observable);
            
            // Assert
            Assert.Equal(expected, (await actual).ChunkSizeInBytes);
        }

        [Fact]
        public void HandleChunkHeader_Smoke_True()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var observable = new Subject<IChunkHeader>();

            // Act
            naiveHeaderCache.HandleChunkHeader(
                observable);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void HandleFileHeader_WhenCacheCold_DoesNotEmit()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var observable = new BehaviorSubject<IFileHeader>(mockFileHeader.Object);

            mockFileHeader
                .SetupGet(fh => fh.FileName)
                .Returns("file name");
            mockFileHeader
                .SetupGet(fh => fh.BroadcastId)
                .Returns(Guid.Empty);

            mockMemoryCache
                .SetupGetOrCreate<HeaderCacheKey, HeaderCacheValue>(mockCacheEntry);

            mockCacheEntry
                .SetupSet(ce=>ce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();
            
            mockMessageCacheConfig
                .SetupGet(mcc => mcc.BroadcastCacheExpiration)
                .Returns(TimeSpan.MaxValue);

            bool actual = false; 
            naiveHeaderCache
                .FileReadyObservable
                .Subscribe(fr=>actual = true);

            // Act
            naiveHeaderCache.HandleFileHeader(
                    observable);
            
            // Assert
            Assert.False(actual);
        }

        [Fact]
        public async Task HandleFileHeader_WhenCached_EmitsFilenameReady()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var observable = new BehaviorSubject<IFileHeader>(mockFileHeader.Object);

            string expected = "file name";
            mockFileHeader
                .SetupGet(fh => fh.FileName)
                .Returns(expected);
            mockFileHeader
                .SetupGet(fh => fh.BroadcastId)
                .Returns(Guid.Empty);

            mockMemoryCache
                .SetupGetOrCreate<HeaderCacheKey, HeaderCacheValue>(mockCacheEntry, new HeaderCacheValue(Guid.Empty){ChunkSizeInBytes = 999 });
            
            var actual = naiveHeaderCache
                .FileReadyObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            naiveHeaderCache.HandleFileHeader(
                    observable);
            
            // Assert
            Assert.Equal(expected, (await actual).FileName);
        }

        [Fact]
        public async Task GetFileName_WhenCacheCold_ReturnsNull()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();

            object nullEntry = null;
            mockMemoryCache
                .Setup(mc=>mc.TryGetValue((object)It.IsAny<HeaderCacheKey>(), out nullEntry))
                .Returns(false);

            // Act
            var actual = naiveHeaderCache.GetFileName(Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541"), 0, 0);

            //Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task GetFileName_WhenCacheDoesNotHaveFileName_ReturnsNull()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");

            object headerValue = new HeaderCacheValue(broadcastId);
            mockMemoryCache
                .Setup(mc=>mc.TryGetValue((object)It.IsAny<HeaderCacheKey>(), out headerValue))
                .Returns(false);

            // Act            
            var actual = naiveHeaderCache.GetFileName(broadcastId, 0, 0);

            //Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task GetFileName_WhenCacheHasFileName_ReturnsFilename()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var broadcastId = Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541");
            var expected = "File Name";

            object headerValue = new HeaderCacheValue(broadcastId){ FileName = expected };
            mockMemoryCache
                .Setup(mc=>mc.TryGetValue((object)It.IsAny<HeaderCacheKey>(), out headerValue))
                .Returns(true);

            // Act            
            var actual = naiveHeaderCache.GetFileName(broadcastId, 0, 0);

            //Assert
            Assert.Equal(expected,actual);
        }
    }
}
