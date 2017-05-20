using System;
using Library.Configuration;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Throttle
{
    public class BroadcastThrottleService_AlmostThrottledShould
    {
        private readonly Mock<IThrottleStateFactory> _mockThrottleStateFactory;
        private readonly BroadcastThrottleService _broadcastThrottleService;
        private readonly Mock<IConfigurationService> _mockConfigurationService;

        public BroadcastThrottleService_AlmostThrottledShould()
        {
            const int maxBroadcastsPerSecond = 2;

            _mockThrottleStateFactory = new Mock<IThrottleStateFactory>();
            _mockThrottleStateFactory
                .Setup(br => br.GeThrottleState())
                .Returns(new ThrottleState
                {
                    BroadcastsCountFromPreviousSecond = maxBroadcastsPerSecond - 1,
                    Paused = false
                });

            _mockConfigurationService = new Mock<IConfigurationService>();

            _mockConfigurationService
                .SetupGet(cs => cs.MaxBroadcastsPerSecond)
                .Returns(maxBroadcastsPerSecond);


            _broadcastThrottleService = new BroadcastThrottleService(_mockThrottleStateFactory.Object,
                _mockConfigurationService.Object);
        }

        [Fact]
        public void RaisePausedWhenTooManyAttemptsPerSecond()
        {
            //assemble
            const string expected = nameof(_broadcastThrottleService.Paused);
            string actual = null;
            _broadcastThrottleService.PropertyChanged +=
                (object sender, System.ComponentModel.PropertyChangedEventArgs e) => { actual = e.PropertyName; };

            //act
            _broadcastThrottleService.Record();

            //assert
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void PauseWhenTooManyAttemptsPerSecond()
        {
            //assemble
            const bool expected = true;

            //act
            _broadcastThrottleService.Record();
            var actual = _broadcastThrottleService.Paused;
            
            //assert
            Assert.Equal(expected, actual);
        }
    }
}