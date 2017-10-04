using System;
using WpfPractice.DataModel;

namespace WpfPractice.Listen
{
    public interface IListenService
    {
        IObservable<BroadcastViewmodelParams> NewBroadcast { get; }
        IObservable<SliverChangedParams> SliverChanged { get; }
    }
}