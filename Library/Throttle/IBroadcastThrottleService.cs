using System.ComponentModel;

namespace Library.Throttle
{
    public interface IBroadcastThrottleService : INotifyPropertyChanged
    {
        void AddCount();
        bool Paused { get; }
    }
}