using System;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using WpfPractice.BroadcastSliver;

namespace WpfPractice.Listen
{
    public class BroadcastViewmodel : ViewmodelBase, IBroadcastViewmodel
    {
        private readonly IListenService _listenService;
        public Guid BroadcastId { get; }
        public ObservableCollection<IBroadcastSliverViewmodel> BroadcastSlivers { get; }

        public BroadcastViewmodel(IListenService listenService, 
            Guid broadcastId)
        {
            _listenService = listenService;
            BroadcastId = broadcastId;
            BroadcastSlivers = new ObservableCollection<IBroadcastSliverViewmodel>();

            for (int i = 0; i < 100; i++)
            {
                BroadcastSlivers.Add(new Mock.MockBroadcastSliverViewmodel());
            }
            
        }

        

        
    }
}