using System;
using System.Threading.Tasks;
using Library.Broadcast;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Broadcast
{
    public class LanService_ThrottledShould
    {
        private readonly LanService _lanService;
        private readonly Mock<ILanRepository> _mockLanRepository;
        private readonly Mock<IBroadcastThrottleService> _mockBroadcastThrottleService;


        public LanService_ThrottledShould()
        {
            _mockLanRepository = new Mock<ILanRepository>();
            _mockLanRepository.Setup(lr => lr.Broadcast(It.IsAny<byte []>()))
                .Returns(Task.CompletedTask);

            _mockBroadcastThrottleService = new Mock<IBroadcastThrottleService>();
            _mockBroadcastThrottleService
                .Setup(bs => bs.Record());
            _mockBroadcastThrottleService
                .SetupGet(bs => bs.Paused)
                .Returns(true);
            _lanService = new LanService(_mockLanRepository.Object, _mockBroadcastThrottleService.Object);
        }

        [Fact]
        public void Broadcast_NotCallBroadcast()
        {
            //assemble


            //act
            byte[] ignoredBytes = { };
            _lanService.Broadcast(ignoredBytes);

            //assert
            _mockLanRepository.Verify(bs => bs.Broadcast(It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
        public void Broadcast_GetsQueued()
        {
            //assemble


            //act
            byte[] ignoredBytes = { };
            _lanService.Broadcast(ignoredBytes);

            //assert
            _mockLanRepository.Verify(lr=>lr.AddToQueue(It.IsAny<byte[]>()));
        }
    }
}