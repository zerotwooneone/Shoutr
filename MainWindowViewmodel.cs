using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Windows;
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
        private WindowState _windowState;
        public ICommand StopCommand { get; }
        public ICommand MouseDownCommand { get; }

        public MainWindowViewmodel(Func<BroadcastViewmodelParams, IObservable<SliverChangedParams>, IBroadcastViewmodel> broadcastViewmodelFactory,
            IListenService listenService)
        {
            Broadcasts = new ObservableCollection<IBroadcastViewmodel>();
            _changes = new ConcurrentDictionary<Guid, Subject<SliverChangedParams>>();

            StopCommand = new RelayCommand(listenService.StopListening);

            MouseDownCommand = new RelayCommand<MouseButtonEventArgs>(args => 
            {
                if (args.ChangedButton != MouseButton.Left) return;
                if (args.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
            });

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
        
        /// <summary>
        /// Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        public WindowState WindowState
        {
            get { return _windowState; }
            set
            {
                if (value == _windowState) return;
                _windowState = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}