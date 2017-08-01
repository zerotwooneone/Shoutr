using System;
using System.Threading.Tasks;
using Library.Broadcast;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Broadcast
{
    /// <summary>
    /// Tests the state in which we just became throttled. The queue is empty and broadcasts should get queued.
    /// </summary>
    public class LanService_WhenThrottled_Should
    {
        private readonly LanService _lanService;
        private readonly Mock<ILanRepository> _mockLanRepository;
        private readonly Mock<IBroadcastThrottleService> _mockBroadcastThrottleService;
        
        public LanService_WhenThrottled_Should()
        {
            _mockLanRepository = new Mock<ILanRepository>();
            _mockLanRepository.Setup(lr => lr.Broadcast(It.IsAny<byte []>()))
                .Returns(Task.CompletedTask);
            _mockLanRepository
                .SetupGet(lr => lr.QueueIsEmpty)
                .Returns(true);
            _mockLanRepository
                .Setup(lr => lr.PopQueue())
                .Throws<InvalidOperationException>();

            _mockBroadcastThrottleService = new Mock<IBroadcastThrottleService>();
            _mockBroadcastThrottleService
                .SetupGet(bs => bs.Paused)
                .Returns(true);
            _lanService = new LanService(_mockLanRepository.Object, _mockBroadcastThrottleService.Object);
        }

        [Fact]
        public void Broadcast_WillNotCallBroadcast()
        {
            //assemble


            //act
            _lanService.Broadcast(It.IsAny<byte[]>());

            //assert
            _mockLanRepository.Verify(bs => bs.Broadcast(It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
        public void Broadcast_GetsQueued()
        {
            //assemble


            //act
            _lanService.Broadcast(It.IsAny<byte[]>());

            //assert
            _mockLanRepository.Verify(lr=>lr.AddToQueue(It.IsAny<byte[]>()));
        }
    }
}