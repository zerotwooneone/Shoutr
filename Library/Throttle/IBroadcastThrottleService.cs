using System.ComponentModel;

namespace Library.Throttle
{
    public interface IBroadcastThrottleService : INotifyPropertyChanged
    {
        void Record();
        bool Paused { get; }
    }
}