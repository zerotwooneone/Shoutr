using Library.Broadcast;
using Moq;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Library.Interface.Broadcast;
using Library.Interface.Configuration;
using Library.Interface.File;
using Library.Interface.Message;
using Library.Interface.Reactive;
using Library.Tests.File;
using Xunit;

namespace Library.Tests.Broadcast
{
    public class NaiveBroadcastServiceTests : IDisposable
    {
        private MockRepository _mockRepository;

        private Mock<IBroadcastMessageObservableFactory> _broadcastMessageObservableFactory;
        private readonly Mock<IBroadcaster> _mockBroadcaster;
        private readonly Mock<IFileMessageConfig> _mockFileMessageConfig;
        private readonly Mock<IConfigurationService> _mockConfigurationService;
        private readonly TestScheduler _testScheduler;

        public NaiveBroadcastServiceTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);

            _broadcastMessageObservableFactory = this._mockRepository.Create<IBroadcastMessageObservableFactory>();
            _mockBroadcaster = _mockRepository.Create<IBroadcaster>();
            _mockFileMessageConfig = _mockRepository.Create<IFileMessageConfig>();
            _mockConfigurationService = _mockRepository.Create<IConfigurationService>();
            _testScheduler = new TestScheduler();
        }

        public void Dispose()
        {
            this._mockRepository.VerifyAll();
        }

        private NaiveBroadcastService CreateService()
        {
            return new NaiveBroadcastService(
                this._broadcastMessageObservableFactory.Object,
                _mockConfigurationService.Object);
        }

        [Fact]
        public async Task BroadcastFile_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();

            _mockConfigurationService
                .SetupGet(cs=>cs.TimeBetweenBroadcasts)
                .Returns(TimeSpan.FromMilliseconds(1));

            _mockBroadcaster
                .Setup(mb => mb.Broadcast(It.IsAny<IBroadcastHeader>()))
                .Returns(Task.CompletedTask);

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
