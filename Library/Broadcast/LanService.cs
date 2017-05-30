using System.Threading.Tasks;
using Library.Throttle;

namespace Library.Broadcast
{
    public class LanService: ILanService
    {
        private readonly ILanRepository _lanRepository;
        private readonly IBroadcastThrottleService _broadcastThrottleService;

        public LanService(ILanRepository lanRepository, IBroadcastThrottleService broadcastThrottleService)
        {
            _lanRepository = lanRepository;
            _broadcastThrottleService = broadcastThrottleService;
        }

        public Task Broadcast(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
    }
}
