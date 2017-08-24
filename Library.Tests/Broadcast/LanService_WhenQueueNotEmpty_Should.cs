using System;
using System.Threading.Tasks;
using Library.Broadcast;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Broadcast
{
    public class LanService_WhenQueueNotEmptyAndDequeueStarted_Should
    {
        private readonly LanService _lanService;
        private readonly Mock<ILanRepository> _mockLanRepository;
        private readonly Mock<IBroadcastThrottleService> _mockBroadcastThrottleService;
        private readonly Task _dequeueTask;

        public LanService_WhenQueueNotEmptyAndDequeueStarted_Should()
        {
            _mockLanRepository = new Mock<ILanRepository>();
            _mockLanRepository
                .SetupGet(lr => lr.QueueIsEmpty)
                .Returns(false); 
            _mockLanRepository
                .Setup(lr => lr.PopQueue())
                .Returns(It.IsAny<byte[]>);
            _dequeueTask = Task.Run(async ()=>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
            _mockLanRepository
                .SetupGet(lr => lr.DequeueTask)
                .Returns(_dequeueTask); //the mock task is already started, and will not complete during our tests

            _mockBroadcastThrottleService = new Mock<IBroadcastThrottleService>();
            _mockBroadcastThrottleService
                .SetupGet(bs => bs.Paused)
                .Returns(false);

            _lanService = new LanService(_mockLanRepository.Object, _mockBroadcastThrottleService.Object);
        }
        
        [Fact]
        public void Dequeue_WillCallRecord()
        {
            //assemble
            _mockBroadcastThrottleService.ResetCalls();

            //act
            _lanService.Dequeue();

            //assert
            _mockBroadcastThrottleService.Verify(bs => bs.Record());
        }

        [Fact]
        public void Dequeue_WillCallRepositoryBroadcast()
        {
            //assemble
            _mockLanRepository.ResetCalls();

            //act
            _lanService.Dequeue();

            //assert
            _mockLanRepository.Verify(bs => bs.Broadcast(It.IsAny<byte[]>()));
        }

        [Fact]
        public void DequeueInProgress_WillBeTrue()
        {
            //assemble
            const bool expected = true;

            //act
            var actual = _lanService.DequeueInProgress;

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Broadcast_WillNotStartDequeueTask()
        {
            //assemble
            _mockLanRepository.ResetCalls();

            //act
            _lanService.Broadcast(It.IsAny<byte[]>());

            //assert
            _mockLanRepository.VerifySet(lr=>lr.DequeueTask=It.IsAny<Task>(), 
                Times.Never);
        }
    }
}