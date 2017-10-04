using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            _sliverChanges = new Dictionary<uint, Subject<SliverChangedParams>>();

            BroadcastId = broadcastViewmodelParams.BroadcastId;
            var slivers = broadcastViewmodelParams
                .Slivers
                .Select(sliver =>
                {
                    _sliverChanges[sliver.SliverIndex] = new Subject<SliverChangedParams>();
                    var sliverViewmodel = broadcastSliverFactory(sliver, _sliverChanges[sliver.SliverIndex]);
                    return sliverViewmodel;
                });
            BroadcastSlivers = new ObservableCollection<IBroadcastSliverViewmodel>(slivers);
            
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