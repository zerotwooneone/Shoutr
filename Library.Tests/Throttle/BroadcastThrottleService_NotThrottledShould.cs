using System;
using Library.Interface.Configuration;
using Library.Interface.Throttle;
using Library.Throttle;
using Moq;
using Xunit;

namespace Library.Tests.Throttle
{
    public class BroadcastThrottleService_WhenOneBroadcastFromBeingThrottled_Should
    {
        private readonly Mock<IThrottleStateRepository> _mockThrottleStateRepository;
        private readonly BroadcastThrottleService _broadcastThrottleService;
        private readonly Mock<IConfigurationService> _mockConfigurationService;

        public BroadcastThrottleService_WhenOneBroadcastFromBeingThrottled_Should()
        {
            const int maxBroadcastsPerSecond = 2;

            _mockThrottleStateRepository = new Mock<IThrottleStateRepository>();
            DateTime now = DateTime.FromBinary(1297380023295);
            _mockThrottleStateRepository
                .Setup(tr => tr.GetDateTime())
                .Returns(now);
            _mockThrottleStateRepository
                .Setup(br => br.GetRecords())
                .Returns(new []
                {
                    now
                });

            _mockConfigurationService = new Mock<IConfigurationService>();

            _mockConfigurationService
                .SetupGet(cs => cs.MaxBroadcastsPerSecond)
                .Returns(maxBroadcastsPerSecond);


            _broadcastThrottleService = new BroadcastThrottleService(_mockThrottleStateRepository.Object,
                _mockConfigurationService.Object);
        }

        [Fact]
        public void RaisePausedWhenRecordCalled()
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
        public void PauseWhenRecordCalled()
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