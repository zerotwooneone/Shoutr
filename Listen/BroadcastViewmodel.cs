using System;

namespace WpfPractice.Listen
{
    public class BroadcastViewmodel : IBroadcastViewmodel
    {
        private readonly IListenService _listenService;
        public Guid BroadcastId { get; }

        public BroadcastViewmodel(IListenService listenService)
        {
            _listenService = listenService;
            BroadcastId = _listenService.GetNextBroadcastId();
        }
    }
}