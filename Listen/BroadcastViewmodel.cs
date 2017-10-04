using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
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
        public readonly IDictionary<uint, Subject<SliverChangedParams>> _sliverChanges;

        public BroadcastViewmodel(IListenService listenService, 
            BroadcastViewmodelParams broadcastViewmodelParams,
            Func<SliverViewmodelParams, 
                IObservable<SliverChangedParams>,
                IBroadcastSliverViewmodel> broadcastSliverFactory)
        {
            
            _listenService = listenService;
            
            BroadcastId = broadcastViewmodelParams.BroadcastId;
            BroadcastSlivers = new ObservableCollection<IBroadcastSliverViewmodel>();

            _sliverChanges = new Dictionary<uint, Subject<SliverChangedParams>>();

            //convert to IEnumerable and pass to observable collection constructor
            foreach (var sliver in broadcastViewmodelParams.Slivers)
            {
                _sliverChanges[sliver.SliverIndex] = new Subject<SliverChangedParams>();
                var sliverViewmodel = broadcastSliverFactory(sliver, _sliverChanges[sliver.SliverIndex]);
                BroadcastSlivers.Add(sliverViewmodel);
            }

            _listenService
                .SliverChanged
                .Subscribe(sp =>
                {
                    if (sp.BroadcastId == BroadcastId)
                    {
                        _sliverChanges[sp.SliverIndex]
                            .OnNext(sp);
                    }
                });
        }
    }
}