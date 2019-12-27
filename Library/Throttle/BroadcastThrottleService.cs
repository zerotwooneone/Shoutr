using System.ComponentModel;
using Library.Interface.Configuration;
using Library.Interface.Throttle;

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

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void Record()
        {
            Paused = true;
            NotifyPropertyChanged(nameof(Paused));
        }

        public bool Paused { get; set; }
    }
}