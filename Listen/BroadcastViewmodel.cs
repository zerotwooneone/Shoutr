using System;

namespace WpfPractice.Listen
{
    public class BroadcastViewmodel : IBroadcastViewmodel
    {
        private readonly IListenService _listenService;
        public Guid BroadcastId { get; }

        public BroadcastViewmodel(IListenService listenService, Guid broadcastId)
        {
            _listenService = listenService;
            BroadcastId = broadcastId;
        }
    }
}