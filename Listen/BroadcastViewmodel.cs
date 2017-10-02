using System;
using System.Collections.ObjectModel;
using WpfPractice.BroadcastSliver;

namespace WpfPractice.Listen
{
    public class BroadcastViewmodel : IBroadcastViewmodel
    {
        private readonly IListenService _listenService;
        public Guid BroadcastId { get; }
        public ObservableCollection<IBroadcastSliverViewmodel> BroadcastSlivers { get; }

        public BroadcastViewmodel(IListenService listenService, Guid broadcastId)
        {
            _listenService = listenService;
            BroadcastId = broadcastId;
            BroadcastSlivers = new ObservableCollection<IBroadcastSliverViewmodel>();
        }
    }
}