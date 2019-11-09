using Library.Broadcast;
using Library.File;
using Library.Message;
using Microsoft.Reactive.Testing;
using Moq;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Library.Tests.Broadcast
{
    public class NaiveBroadcastServiceTests : IDisposable
    {
        private MockRepository _mockRepository;

        private Mock<IBroadcastMessageObservableFactory> _broadcastMessageObservableFactory;
        private readonly Mock<IBroadcaster> _mockBroadcaster;
        private readonly Mock<IFileMessageConfig> _mockFileMessageConfig;
        private readonly TestScheduler _testScheduler;

        public NaiveBroadcastServiceTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);

            _broadcastMessageObservableFactory = this._mockRepository.Create<IBroadcastMessageObservableFactory>();
            _mockBroadcaster = _mockRepository.Create<IBroadcaster>();
            _mockFileMessageConfig = _mockRepository.Create<IFileMessageConfig>();
            _testScheduler = new TestScheduler();
        }

        public void Dispose()
        {
            this._mockRepository.VerifyAll();
        }

        private NaiveBroadcastService CreateService()
        {
            return new NaiveBroadcastService(
                this._broadcastMessageObservableFactory.Object);
        }

        [Fact]
        public async Task BroadcastFile_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();

            _mockBroadcaster
                .Setup(mb => mb.Broadcast(It.IsAny<IBroadcastHeader>()))
                .Returns(Task.CompletedTask);

            //_mockBroadcaster
            //    .Setup(mb => mb.Broadcast(It.IsAny<IFileHeader>()))
            //    .Returns(Task.CompletedTask);

            //_mockBroadcaster
            //    .Setup(mb=>mb.Broadcast(It.IsAny<IPayloadMessage>()))
            //    .Returns(Task.CompletedTask);

            var broadcastId = Guid.Empty;
            var fileName = "file name";
            _broadcastMessageObservableFactory
                .Setup(mof => mof.GetFileBroadcast(It.IsAny<string>(),
                    It.IsAny<IFileMessageConfig>(),
                    It.IsAny<IScheduler>(),
                    It.IsAny<Guid>()))
                .Returns(Observable.Return<IMessages>(new Messages() { BroadcastHeader = new BroadcastHeader(broadcastId, false, 999) }));
            
            // Act
            var resultObservable = service.BroadcastFile(
                fileName,
                _mockFileMessageConfig.Object,
                _mockBroadcaster.Object,
                _testScheduler);

            _testScheduler.Start();

            await resultObservable;

            // Assert
            _mockBroadcaster
                .Verify(mb => mb.Broadcast(It.IsAny<IBroadcastHeader>()), Times.Once);
        }
    }
}
