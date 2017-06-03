using System.ComponentModel;
using System.Threading.Tasks;
using Library.Broadcast;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Broadcast
{
    public class LanService_WhenQueueNotEmpty_Should
    {
        private readonly LanService _lanService;
        private readonly Mock<ILanRepository> _mockLanRepository;
        private readonly Mock<IBroadcastThrottleService> _mockBroadcastThrottleService;
        
        public LanService_WhenQueueNotEmpty_Should()
        {
            _mockLanRepository = new Mock<ILanRepository>();
            _mockLanRepository.SetupGet(lr => lr.QueueIsEmpty)
                .Returns(false);
            _mockLanRepository.Setup(lr => lr.PopQueue())
                .Returns(Task.CompletedTask);

            _mockBroadcastThrottleService = new Mock<IBroadcastThrottleService>();
            _mockBroadcastThrottleService
                .Setup(bs => bs.Record());
            _mockBroadcastThrottleService
                .SetupGet(bs => bs.Paused)
                .Returns(false);
            _lanService = new LanService(_mockLanRepository.Object, _mockBroadcastThrottleService.Object);
        }

        [Fact]
        public void BroadcastQueuedWhenUnpaused()
        {
            //assemble


            //act
            _mockBroadcastThrottleService.Raise(ts => ts.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IBroadcastThrottleService.Paused)));

            //assert
            _mockLanRepository.Verify(lr=>lr.Broadcast(It.IsAny<byte[]>()));
        }

        [Fact]
        public void DrainQueueWhenUnpaused()
        {
            //assemble


            //act
            _mockBroadcastThrottleService.Raise(ts => ts.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IBroadcastThrottleService.Paused)));

            //assert
            _mockLanRepository.Verify(lr => lr.PopQueue());
        }

        [Fact]
        public void QueueInsteadOfBroadcast()
        {
            //assemble
            var expectedBytes = new byte[] {124};

            //act
            _lanService.Broadcast(expectedBytes);

            //assert
            _mockLanRepository.Verify(lr => lr.AddToQueue(It.IsAny<Task>()));
        }


    }
}