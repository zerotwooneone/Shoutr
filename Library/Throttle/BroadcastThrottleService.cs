using System.ComponentModel;
using Library.Configuration;

namespace Library.Throttle
{
    public class BroadcastThrottleService : IBroadcastThrottleService
    {
        private readonly IThrottleStateRepository _throttleStateRepository;
        private readonly IConfigurationService _configurationService;

        public BroadcastThrottleService(IThrottleStateRepository throttleStateRepository,
            IConfigurationService configurationService)
        {
            _throttleStateRepository = throttleStateRepository;
            _configurationService = configurationService;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Record()
        {
            throw new System.NotImplementedException();
        }

        public bool Paused { get; }
    }
}