using System;
using System.Collections;
using System.Collections.Concurrent;
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
                IBroadcastSliverViewmodel> broadcastSliverFactory,
            IObservable<SliverChangedParams> sliverChanged)
        {

            _listenService = listenService;
            _sliverChanges = new ConcurrentDictionary<uint, Subject<SliverChangedParams>>();

            BroadcastId = broadcastViewmodelParams.BroadcastId;
            var slivers = broadcastViewmodelParams
                .Slivers
                .Select(sliver =>
                {
                    var sliverChange = new Subject<SliverChangedParams>();
                    _sliverChanges[sliver.SliverIndex] = sliverChange;
                    var sliverViewmodel = broadcastSliverFactory(sliver, sliverChange);
                    return sliverViewmodel;
                });
            BroadcastSlivers = new ObservableCollection<IBroadcastSliverViewmodel>(slivers);
            
            sliverChanged
                .Subscribe(sp =>
                {
                    _sliverChanges[sp.SliverIndex]
                            .OnNext(sp);
                });
        }
    }
}