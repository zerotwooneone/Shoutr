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
                    Dequeue();
                }
            });
            _broadcastThrottleService.PropertyChanged += _broadcastThrottleService_PropertyChanged;
        }

        private void _broadcastThrottleService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IBroadcastThrottleService.Paused) && !_broadcastThrottleService.Paused)
            {

            }
        }

        public Task Broadcast(byte[] bytes)
        {
            if (_broadcastThrottleService.Paused)
            {
                _lanRepository.AddToQueue(bytes);
                return null; //this is a problem for tomorrow Paz
            }
            else
            {
                _broadcastThrottleService.Record();
                return _lanRepository.Broadcast(bytes);
            }
        }

        public bool ShouldDequeue => !_broadcastThrottleService.Paused && !_lanRepository.QueueIsEmpty;
        public void Dequeue()
        {
            var t = _lanRepository.PopQueue();
            _broadcastThrottleService.Record();
            _lanRepository.Broadcast(t);
        }
    }
}