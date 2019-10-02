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
        public void Handle_BroadcastSmoke_True()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var observable = new Subject<IBroadcastHeader>();

            // Act
            naiveHeaderCache.Handle(
                observable);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void Handle_ChunkSmoke_True()
        {
            // Arrange
            var naiveHeaderCache = this.CreateNaiveHeaderCache();
            var observable = new Subject<IChunkHeader>();

            // Act
            naiveHeaderCache.Handle(
                observable);

            // Assert
            Assert.True(false);
        }

        [Fact]
        public async Task Handle_FileHeaderWhenCacheCold_EmitsFilenameReady()
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
                .Returns(Guid.Parse("58f5022e-0024-4105-a0b8-9c77b0ead541"));

            mockMemoryCache
                .SetupGetOrCreate<NaiveHeaderCache.HeaderCacheKey, NaiveHeaderCache.HeaderCacheValue>(mockCacheEntry);

            mockCacheEntry
                .SetupSet(ce=>ce.SlidingExpiration = It.IsAny<TimeSpan?>())
                .Verifiable();
            
            mockMessageCacheConfig
                .SetupGet(mcc => mcc.BroadcastCacheExpiration)
                .Returns(TimeSpan.MaxValue);

            var actual = naiveHeaderCache
                .FileReadyObservable
                .FirstOrDefaultAsync()
                .ToTask();

            // Act
            naiveHeaderCache.Handle(
                    observable);
            
            // Assert
            Assert.Equal(expected, (await actual).FileName);
        }
    }
}
