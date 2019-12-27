using System.ComponentModel;

namespace Library.Interface.Throttle
{
    /// <summary>
    /// Keeps track of how many broadcast have recently sent and emits events when we should be throttling our broadcasts to stay within the configured limits.
    /// </summary>
    public interface IBroadcastThrottleService : INotifyPropertyChanged
    {
        void Record();
        bool Paused { get; }
    }
}