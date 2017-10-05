using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Windows.Threading;
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
            _changes = new ConcurrentDictionary<Guid, Subject<SliverChangedParams>>();

            _broadcastViewmodelFactory = broadcastViewmodelFactory;
            _listenService = listenService;
            
            _listenService
                .NewBroadcast
                .Subscribe(p =>
                {
                    var subject = new Subject<SliverChangedParams>();
                    _changes[p.BroadcastId] = subject;
                    var broadcast = _broadcastViewmodelFactory(p, subject);

                    App.Current.Dispatcher.Invoke(()=>
                    {
                        Broadcasts.Add(broadcast);
                    });
                });
            _listenService
                .SliverChanged
                .Subscribe(sc =>
                {
                    _changes[sc.BroadcastId]
                        .OnNext(sc);
                });
        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}