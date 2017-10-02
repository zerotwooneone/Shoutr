using System;
using System.Collections.ObjectModel;
using WpfPractice.BroadcastSliver;
using WpfPractice.Listen;

namespace WpfPractice.Mock
{
    public class MockListenBroadcastViewmodel : IBroadcastViewmodel
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