using System;
using System.Collections.ObjectModel;
using WpfPractice.DataModel;
using WpfPractice.Listen;
using WpfPractice.Viewmodel;

namespace WpfPractice
{
    public class MainWindowViewmodel : ViewmodelBase, IMainWindowViewmodel
    {
        private readonly Func<BroadcastViewmodelParams, IBroadcastViewmodel> _broadcastViewmodelFactory;
        private readonly IListenService _listenService;

        public MainWindowViewmodel(Func<BroadcastViewmodelParams, IBroadcastViewmodel> broadcastViewmodelFactory,
            IListenService listenService)
        {
            Broadcasts = new ObservableCollection<IBroadcastViewmodel>();

            _broadcastViewmodelFactory = broadcastViewmodelFactory;
            _listenService = listenService;
            _listenService
                .NewBroadcast
                .Subscribe(p =>
                {
                    var broadcast = _broadcastViewmodelFactory(p);
                    Broadcasts.Add(broadcast);
                });
        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}