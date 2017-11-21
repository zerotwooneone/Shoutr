using System;
using System.Collections.ObjectModel;
using WpfPractice.BroadcastSliver;
using WpfPractice.Listen;
using WpfPractice.Viewmodel;

namespace WpfPractice.Mock
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