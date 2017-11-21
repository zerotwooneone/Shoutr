using System;
using System.Collections.ObjectModel;
using WpfPractice.BroadcastSliver;

namespace WpfPractice.Listen
{
    public interface IBroadcastViewmodel
    {
        Guid BroadcastId { get; }
        ObservableCollection<IBroadcastSliverViewmodel> BroadcastSlivers { get; }
    }
}