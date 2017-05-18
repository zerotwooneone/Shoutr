namespace Library.Throttle
{
    public interface IBroadcastThrottleService
    {
        bool TryBroadcast(int port);
    }
}