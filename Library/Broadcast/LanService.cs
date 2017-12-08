using System;
using System.Threading.Tasks;
using Library.Throttle;

namespace Library.Broadcast
{
    public class LanService : ILanService
    {
        private readonly ILanRepository _lanRepository;
        private readonly IBroadcastThrottleService _broadcastThrottleService;
        [Obsolete("This task is now located on the repository. This should be removed.")]
        private readonly Task _DrainQueue;

        public LanService(ILanRepository lanRepository, IBroadcastThrottleService broadcastThrottleService)
        {
            _lanRepository = lanRepository;
            _broadcastThrottleService = broadcastThrottleService;

            //we are going to start the task when broadcast is called, if the task is not allready started, so we dont need this here.
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

        public void Broadcast(byte[] bytes)
        {
            if (_broadcastThrottleService.Paused)
            {
                _lanRepository.AddToQueue(bytes);
            }
            else
            {
                _broadcastThrottleService.Record();
                _lanRepository.Broadcast(bytes);
            }
        }

        [Obsolete("Depricated. This should be removed.")]
        public bool ShouldDequeue => !_broadcastThrottleService.Paused && !_lanRepository.QueueIsEmpty;

        public bool DequeueInProgress => throw new NotImplementedException();

        public void Dequeue()
        {
            var t = _lanRepository.PopQueue();
            _broadcastThrottleService.Record();
            _lanRepository.Broadcast(t);
        }

        public Task StartBroadcast()
        {
            throw new NotImplementedException();
        }
    }
}