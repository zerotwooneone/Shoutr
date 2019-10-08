using Library.Message;
using Moq;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace Library.Tests.Message
{
    public class NaiveMessageCacheTests : IDisposable
    {
        private MockRepository mockRepository;

        private Mock<IHeaderCache> mockHeaderCache;
        private Mock<IPayloadCache> mockPayloadCache;

        public NaiveMessageCacheTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockHeaderCache = this.mockRepository.Create<IHeaderCache>();
            this.mockPayloadCache = this.mockRepository.Create<IPayloadCache>();
        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        private NaiveMessageCache CreateNaiveMessageCache()
        {
            return new NaiveMessageCache(
                this.mockHeaderCache.Object,
                this.mockPayloadCache.Object);
        }

        [Fact]
        public void Handle_SomeState_CallsHandleFileReady()
        {
            // Arrange
            var naiveMessageCache = this.CreateNaiveMessageCache();
            var messagesObservable = new Subject<IReceivedMessage>();

            mockHeaderCache
                .Setup(hc=>hc.HandleBroadcastHeader(It.IsAny<IObservable<IBroadcastHeader>>()))
                .Verifiable();
            mockHeaderCache
                .Setup(hc=>hc.HandleFileHeader(It.IsAny<IObservable<IFileHeader>>()))
                .Verifiable();
            mockHeaderCache
                .Setup(hc=>hc.HandleChunkHeader(It.IsAny<IObservable<IChunkHeader>>()))
                .Verifiable();
            var fileReadyObservable = new Subject<IFileReadyMessage>().AsObservable();
            mockHeaderCache
                .SetupGet(hc => hc.FileReadyObservable)
                .Returns(fileReadyObservable);

            mockPayloadCache
                .Setup(pc=>pc.HandlePayload(It.IsAny<IObservable<IPayloadMessage>>()))
                .Verifiable();
            mockPayloadCache
                .Setup(pc=>pc.HandleFileReady(It.IsAny<IObservable<IFileReadyMessage>>()))
                .Verifiable();

            // Act
            naiveMessageCache.Handle(
                messagesObservable);

            // Assert
            mockPayloadCache
                .Verify(pc=>pc.HandleFileReady(It.Is<IObservable<IFileReadyMessage>>(o=>fileReadyObservable.Equals(o))),
                    Times.Once);
        }
    }
}
