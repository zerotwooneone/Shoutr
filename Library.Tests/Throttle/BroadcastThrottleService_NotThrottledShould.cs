using System;
using Library.Throttle;
using Moq;

namespace Library.Tests.Throttle
{
    public class BroadcastThrottleService_NotThrottledShould
    {
        private readonly Mock<IBroadcastThrottleRepository> _mockBroadcastThrottleRepository;
        private readonly BroadcastThrottleService _broadcastThrottleService;


        public BroadcastThrottleService_NotThrottledShould()
        {
            _mockBroadcastThrottleRepository = new Mock<IBroadcastThrottleRepository>();
            _mockBroadcastThrottleRepository
                .Setup(br => br.SaveAttempt(It.IsAny<int>(), It.IsAny<DateTime>()))
                .Returns(new BroadcastAttempt {Timeout = null});

            _broadcastThrottleService = new BroadcastThrottleService(_mockBroadcastThrottleRepository.Object);
        }

        
    }
}