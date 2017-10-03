using System;
using System.Collections.ObjectModel;
using WpfPractice.BroadcastSliver;
using WpfPractice.DataModel;
using WpfPractice.Viewmodel;

namespace WpfPractice.Listen
{
    public class BroadcastViewmodel : ViewmodelBase, IBroadcastViewmodel
    {
        private readonly IListenService _listenService;
        public Guid BroadcastId { get; }
        public ObservableCollection<IBroadcastSliverViewmodel> BroadcastSlivers { get; }

        public BroadcastViewmodel(IListenService listenService, 
            BroadcastViewmodelParams broadcastViewmodelParams,
            Func<SliverViewmodelParams, 
                IBroadcastSliverViewmodel> broadcastSliverFactory)
        {
            _listenService = listenService;
            BroadcastId = broadcastViewmodelParams.BroadcastId;
            BroadcastSlivers = new ObservableCollection<IBroadcastSliverViewmodel>();

            foreach (var sliver in broadcastViewmodelParams.Slivers)
            {
                var sliverViewmodel = broadcastSliverFactory(sliver);
                BroadcastSlivers.Add(sliverViewmodel);
            }
        }
    }
}