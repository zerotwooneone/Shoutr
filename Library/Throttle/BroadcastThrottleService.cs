using Library.Broadcast;

namespace Library.Throttle
{
    public class BroadcastThrottleService : IBroadcastThrottleService
    {
        private readonly IBroadcastThrottleRepository _broadcastThrottleRepository;
        public BroadcastThrottleService(IBroadcastThrottleRepository broadcastThrottleRepository)
        {
            _broadcastThrottleRepository = broadcastThrottleRepository;
        }

        public bool TryBroadcast(int port)
        {
            throw new System.NotImplementedException();
        }
    }
}