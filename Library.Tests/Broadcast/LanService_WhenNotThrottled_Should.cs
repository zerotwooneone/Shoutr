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
        private readonly byte[] bytes = It.IsAny<byte[]>();


        public LanService_WhenNotThrottled_Should()
        {
            _mockLanRepository = new Mock<ILanRepository>();
            _mockLanRepository
                .Setup(lr => lr.Broadcast(It.IsAny<byte[]>()))
                .Returns(Task.CompletedTask);
            _mockLanRepository
                .SetupGet(lr => lr.QueueIsEmpty)
                .Returns(true);
            _mockLanRepository
                .Setup(lr => lr.PopQueue())
                .Returns(bytes);
            _mockLanRepository
                .SetupGet(lr => lr.DequeueTask)
                .Returns(Task.CompletedTask); 

            _mockBroadcastThrottleService = new Mock<IBroadcastThrottleService>();
            _mockBroadcastThrottleService
                .SetupGet(bs => bs.Paused)
                .Returns(false);
            _lanService = new LanService(_mockLanRepository.Object, _mockBroadcastThrottleService.Object);
        }

        [Fact]
        public void Broadcast_GetsQueued()
        {
            //assemble
            _mockLanRepository.ResetCalls();

            //act
            _lanService.Broadcast(It.IsAny<byte[]>());

            //assert
            _mockLanRepository.Verify(lr => lr.AddToQueue(It.IsAny<byte[]>()));
        }

        [Fact]
        public void StartBroadcast_SetsNewDequeueTask()
        {
            //assemble
            _mockLanRepository.ResetCalls();

            //act
            _lanService.StartBroadcast();
            //assert
            _mockLanRepository.VerifySet(lr => lr.DequeueTask = It.IsAny<Task>());
        }
        
        [Fact]
        public void SStartBroadcast_StartsNewDequeueTask()
        {
            //assemble
            _mockLanRepository.ResetCalls();

            //act
            _lanService.StartBroadcast();
            //assert
            _mockLanRepository.VerifySet(lr => lr.DequeueTask = It.IsAny<Task>());
        }

        [Fact]
        public void DequeueInProgress_WillBeFalse()
        {
            //assemble
            const bool expected = false;
            //act
            var actual = _lanService.DequeueInProgress;
            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Dequeue_WillCallRepoDequeue()
        {
            //assemble
            _mockLanRepository.ResetCalls();

            //act
            _lanService.Dequeue();

            //assert
            _mockLanRepository.Verify(lr => lr.PopQueue());
        }

        [Fact]
        public void Dequeue_WillCallRepoBroadcast()
        {
            //assemble
            _mockLanRepository.ResetCalls();

            //act
            _lanService.Dequeue();

            //assert
            _mockLanRepository.Verify(lr => lr.Broadcast(It.IsAny<byte[]>()));
        }
    }
}
