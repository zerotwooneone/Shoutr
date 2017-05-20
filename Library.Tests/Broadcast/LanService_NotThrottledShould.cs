using System;
using System.Threading.Tasks;
using Library.Broadcast;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Broadcast
{
    public class LanService_NotThrottledShould
    {
        private readonly LanService _lanService;
        private readonly Mock<ILanRepository> _mockLanRepository;
        private readonly Mock<IBroadcastThrottleService> _mockBroadcastThrottleService;


        public LanService_NotThrottledShould()
        {
            _mockLanRepository = new Mock<ILanRepository>();
            _mockLanRepository.Setup(lr => lr.Broadcast(It.IsAny<Byte[]>()))
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
        public void ReturnTask()
        {
            //assemble
            
            //act
            byte[] ignoredBytes = { };
            var actual = _lanService.Broadcast(ignoredBytes);

            //assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void CallRecord()
        {
            //assemble


            //act
            byte[] ignoredBytes = { };
            _lanService.Broadcast(ignoredBytes);

            //assert
            _mockBroadcastThrottleService.Verify(bs => bs.Record());
        }

        [Fact]
        public void CallBroadcast()
        {
            //assemble


            //act
            byte[] ignoredBytes = { };
            _lanService.Broadcast(ignoredBytes);

            //assert
            _mockLanRepository.Verify(bs => bs.Broadcast(It.IsAny<byte[]>()));
        }
    }
}
