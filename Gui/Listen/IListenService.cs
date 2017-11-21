using System;
using ShoutrGui.DataModel;

namespace ShoutrGui.Listen
{
    public interface IListenService
    {
        IObservable<BroadcastViewmodelParams> NewBroadcast { get; }
        IObservable<SliverChangedParams> SliverChanged { get; }
        void StopListening();
    }
}