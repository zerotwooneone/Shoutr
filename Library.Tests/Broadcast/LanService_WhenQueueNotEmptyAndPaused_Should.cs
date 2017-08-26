using System.ComponentModel;
using Library.Broadcast;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Broadcast
{
    public class LanService_WhenQueueNotEmptyAndPaused_Should
    {
        private readonly LanService _lanService;
        private readonly Mock<ILanRepository> _mockLanRepository;
        private readonly Mock<IBroadcastThrottleService> _mockBroadcastThrottleService;

        public LanService_WhenQueueNotEmptyAndPaused_Should()
        {
            _mockLanRepository = new Mock<ILanRepository>();
            _mockLanRepository
                .SetupGet(lr => lr.QueueIsEmpty)
                .Returns(false);
            _mockLanRepository
                .Setup(lr => lr.PopQueue())
                .Returns(It.IsAny<byte[]>);

            _mockBroadcastThrottleService = new Mock<IBroadcastThrottleService>();
            _mockBroadcastThrottleService
                .SetupGet(bs => bs.Paused)
                .Returns(true);
            _lanService = new LanService(_mockLanRepository.Object, _mockBroadcastThrottleService.Object);
        }
        
        [Fact]
        public void QueueInsteadOfBroadcast()
        {
            //assemble
            _mockLanRepository.ResetCalls();
            var expectedBytes = new byte[] {124};

            //act
            _lanService.Broadcast(expectedBytes);

            //assert
            _mockLanRepository.Verify(lr => lr.AddToQueue(It.IsAny<byte[]>()));
        }
    }
}