using System;
using System.Collections.ObjectModel;
using ShoutrGui.BroadcastSliver;

namespace ShoutrGui.Listen
{
    public interface IBroadcastViewmodel
    {
        Guid BroadcastId { get; }
        ObservableCollection<IBroadcastSliverViewmodel> BroadcastSlivers { get; }
    }
}