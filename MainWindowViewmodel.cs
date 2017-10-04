using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using WpfPractice.DataModel;
using WpfPractice.Listen;
using WpfPractice.Viewmodel;

namespace WpfPractice
{
    public class MainWindowViewmodel : ViewmodelBase, IMainWindowViewmodel
    {
        private readonly Func<BroadcastViewmodelParams, IObservable<SliverChangedParams>, IBroadcastViewmodel> _broadcastViewmodelFactory;
        private readonly IListenService _listenService;
        public readonly IDictionary<Guid, Subject<SliverChangedParams>> _changes;

        public MainWindowViewmodel(Func<BroadcastViewmodelParams, IObservable<SliverChangedParams>, IBroadcastViewmodel> broadcastViewmodelFactory,
            IListenService listenService)
        {
            Broadcasts = new ObservableCollection<IBroadcastViewmodel>();
            _changes = new Dictionary<Guid, Subject<SliverChangedParams>>();

            _broadcastViewmodelFactory = broadcastViewmodelFactory;
            _listenService = listenService;
            _listenService
                .SliverChanged
                .Subscribe(sc =>
                {
                    _changes[sc.BroadcastId]
                        .OnNext(sc);
                });
            _listenService
                .NewBroadcast
                .Subscribe(p =>
                {
                    _changes[p.BroadcastId] = new Subject<SliverChangedParams>();
                    var broadcast = _broadcastViewmodelFactory(p, _changes[p.BroadcastId]);
                    Broadcasts.Add(broadcast);
                });
        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}