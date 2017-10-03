using System;
using System.Reactive.Subjects;
using WpfPractice.DataModel;

namespace WpfPractice.BroadcastSliver
{
    public class BroadcastSliverService : IBroadcastSliverService
    {
        private readonly Subject<SliverChangedParams> _broadcastSliverChanged;

        public BroadcastSliverService()
        {
            _broadcastSliverChanged = new Subject<SliverChangedParams>();
        }

        public IObservable<SliverChangedParams> BroadcastSliverChanged => _broadcastSliverChanged;
        public void Hack(Guid broadcastId, uint sliverIndex, bool success)
        {
            _broadcastSliverChanged
                .OnNext(new SliverChangedParams()
                {
                    BroadcastId = broadcastId,
                    SliverIndex = sliverIndex,
                    Success = success
                });
        }
    }
}