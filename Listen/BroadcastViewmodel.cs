using System;
using System.Collections.ObjectModel;
using System.Windows;
using WpfPractice.BroadcastSliver;

namespace WpfPractice.Listen
{
    public class BroadcastViewmodel : IBroadcastViewmodel
    {
        private readonly IListenService _listenService;
        public Guid BroadcastId { get; }
        public ObservableCollection<IBroadcastSliverViewmodel> BroadcastSlivers { get; }
        public float SliverWidth { get; set; }

        public BroadcastViewmodel(IListenService listenService, 
            Guid broadcastId,
            IResizeService resizeService)
        {
            _listenService = listenService;
            BroadcastId = broadcastId;
            BroadcastSlivers = new ObservableCollection<IBroadcastSliverViewmodel>();

            for (int i = 0; i < 100; i++)
            {
                BroadcastSlivers.Add(new Mock.MockBroadcastSliverViewmodel());
            }
            SliverWidth = 1;
            resizeService.SliverPanelResized += OnSliverPanelResized;
            Console.WriteLine($"broadcast resize:{resizeService.GetHashCode()}");
        }

        private void OnSliverPanelResized(object sender, SizeChangedEventArgs e)
        {
            int x = 0;
        }
    }
}