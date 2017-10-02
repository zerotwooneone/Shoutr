using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfPractice.Listen;

namespace WpfPractice
{
    public class MainWindowViewmodel : IMainWindowViewmodel
    {
        private readonly Func<IBroadcastViewmodel> _broadcastViewmodelFactory;
        private readonly IListenService _listenService;

        public MainWindowViewmodel(Func<IBroadcastViewmodel> broadcastViewmodelFactory,
            IListenService listenService)
        {
            Broadcasts = new ObservableCollection<IBroadcastViewmodel>();

            _broadcastViewmodelFactory = broadcastViewmodelFactory;
            _listenService = listenService;
            _listenService
                .NewBroadcast += OnNewBroadcast;
        }

        private void OnNewBroadcast(object sender, EventArgs e)
        {
            var broadcast = _broadcastViewmodelFactory();
            Broadcasts.Add(broadcast);
        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}