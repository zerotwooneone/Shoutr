using System;
using WpfPractice.DataModel;

namespace WpfPractice.BroadcastSliver
{
    public interface IBroadcastSliverService
    {
        IObservable<SliverChangedParams> BroadcastSliverChanged { get; }
        void Hack(Guid broadcastId, uint sliverIndex, bool success);
    }
}