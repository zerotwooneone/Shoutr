using System;
using System.Collections.ObjectModel;
using ShoutrGui.BroadcastSliver;
using ShoutrGui.Listen;
using ShoutrGui.Viewmodel;

namespace ShoutrGui.Mock
{
    public class MockListenBroadcastViewmodel : ViewmodelBase, IBroadcastViewmodel
    {
        public MockListenBroadcastViewmodel()
        {
            BroadcastId = Guid.NewGuid();
            BroadcastSlivers = new ObservableCollection<IBroadcastSliverViewmodel>();
            for (int i = 0; i < 100; i++)
            {
                BroadcastSlivers.Add(new MockBroadcastSliverViewmodel());
            }
        }

        public Guid BroadcastId { get; }
        public ObservableCollection<IBroadcastSliverViewmodel> BroadcastSlivers { get; }
    }
}