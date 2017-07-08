using System;
using System.Threading.Tasks;
using Library.Throttle;

namespace Library.Broadcast
{
    public class LanService : ILanService
    {
        private readonly ILanRepository _lanRepository;
        private readonly IBroadcastThrottleService _broadcastThrottleService;
        private readonly Task _DrainQueue;

        public LanService(ILanRepository lanRepository, IBroadcastThrottleService broadcastThrottleService)
        {
            _lanRepository = lanRepository;
            _broadcastThrottleService = broadcastThrottleService;
            _DrainQueue = Task.Run(async () => {
                while (ShouldDequeue)
                {
                    Task t = _lanRepository.PopQueue();
                    t.Start();
                }
            });
        }

        public Task Broadcast(byte[] bytes)
        {
            Task t = new Task(async () => { await _lanRepository.Broadcast(bytes); } );
            _lanRepository.AddToQueue(t);
            return t;
        }

        public bool ShouldDequeue => !_broadcastThrottleService.Paused && !_lanRepository.QueueIsEmpty;
        public void Dequeue()
        {
            _broadcastThrottleService.Record();
            _lanRepository.Broadcast(null  /*  <<<<  Needs to pass a byte[]  */  );
        }
    }
}