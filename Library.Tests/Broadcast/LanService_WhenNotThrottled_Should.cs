using System;
using System.Threading.Tasks;
using Library.Broadcast;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Broadcast
{
    public class LanService_WhenNotThrottled_Should
    {
        private readonly LanService _lanService;
        private readonly Mock<ILanRepository> _mockLanRepository;
        private readonly Mock<IBroadcastThrottleService> _mockBroadcastThrottleService;


        public LanService_WhenNotThrottled_Should()
        {
            _mockLanRepository = new Mock<ILanRepository>();
            _mockLanRepository.Setup(lr => lr.Broadcast(It.IsAny<byte[]>()))
                .Returns(Task.CompletedTask);
            _mockLanRepository
                .SetupGet(lr => lr.QueueIsEmpty)
                .Returns(true);
            _mockLanRepository
                .Setup(lr => lr.PopQueue())
                .Returns(Task.CompletedTask);

            _mockBroadcastThrottleService = new Mock<IBroadcastThrottleService>();
            _mockBroadcastThrottleService
                .SetupGet(bs => bs.Paused)
                .Returns(false);
            _lanService = new LanService(_mockLanRepository.Object, _mockBroadcastThrottleService.Object);
        }

        [Fact]
        public void Broadcast_WillReturnTask()
        {
            //assemble
            
            //act
            var actual = _lanService.Broadcast(It.IsAny<byte[]>());

            //assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void Broadcast_WillGetQueued()
        {
            //assemble

            //act
            _lanService.Broadcast(It.IsAny<byte[]>());

            //assert
            _mockLanRepository.Verify(lr=>lr.AddToQueue(It.IsAny<Task>()));
        }

        [Fact]
        public void ShouldDequeue_WillBeFalse()
        {
            //assemble
            const bool expected = false;

            //act
            var actual = _lanService.ShouldDequeue;

            //assert
            Assert.Equal(expected, actual);
        }

        //[Fact]
        //public async Task Broadcast_WillGetDeQueued()
        //{
        //    //assemble

        //    //act
        //    await _lanService.Broadcast(It.IsAny<byte[]>());

        //    //assert
        //    _mockLanRepository.Verify(lr => lr.PopQueue());
        //}

        [Fact]
        public void Dequeue_WillCallRepoDequeue()
        {
            //assemble

            //act
            _lanService.Dequeue();

            //assert
            _mockLanRepository.Verify(lr => lr.PopQueue());
        }

        [Fact]
        public void Dequeue_WillCallRepoBroadcast()
        {
            //assemble

            //act
            _lanService.Dequeue();

            //assert
            _mockLanRepository.Verify(lr => lr.Broadcast(It.IsAny<byte[]>()));
        }
    }
}
