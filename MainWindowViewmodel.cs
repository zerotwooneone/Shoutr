using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using WpfPractice.DataModel;
using WpfPractice.Listen;
using WpfPractice.Viewmodel;

namespace WpfPractice
{
    public class MainWindowViewmodel : ViewmodelBase, IMainWindowViewmodel
    {
        private readonly Func<BroadcastViewmodelParams, IObservable<SliverChangedParams>, IBroadcastViewmodel> _broadcastViewmodelFactory;
        private readonly IListenService _listenService;
        private readonly IDictionary<Guid, Subject<SliverChangedParams>> _changes;
        public ICommand StopCommand { get; }

        public MainWindowViewmodel(Func<BroadcastViewmodelParams, IObservable<SliverChangedParams>, IBroadcastViewmodel> broadcastViewmodelFactory,
            IListenService listenService)
        {
            Broadcasts = new ObservableCollection<IBroadcastViewmodel>();
            _changes = new ConcurrentDictionary<Guid, Subject<SliverChangedParams>>();

            StopCommand = new RelayCommand(listenService.StopListening);

            _broadcastViewmodelFactory = broadcastViewmodelFactory;
            _listenService = listenService;

            _listenService
                .NewBroadcast
                .Subscribe(p =>
                {
                    var subject = new Subject<SliverChangedParams>();
                    _changes[p.BroadcastId] = subject;
                    var broadcast = _broadcastViewmodelFactory(p, subject);

                    App.Current.Dispatcher.Invoke(() =>
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