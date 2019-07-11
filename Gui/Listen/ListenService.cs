using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ShoutrGui.DataModel;
using ShoutrGui.Dispatcher;

namespace ShoutrGui.Listen
{
    public class ListenService : IListenService
    {
        public IObservable<BroadcastViewmodelParams> NewBroadcast { get; }

        private readonly Subject<SliverChangedParams> _sliverChanged;
        public IObservable<SliverChangedParams> SliverChanged => _sliverChanged;
        private bool _doContinue = true;

        public void StopListening()
        {
            _doContinue = false;
        }

        public ListenService()
        {
            //_newBroadcast = new Subject<BroadcastViewmodelParams>();
            _sliverChanged = new Subject<SliverChangedParams>();
            
            var dispatcher = new DispatcherWrapper(System.Windows.Threading.Dispatcher.CurrentDispatcher);

            NewBroadcast = Observable
                .Range(1, 8)
                .Select(broadcastCount =>
                {
                    return Observable.Timer(TimeSpan.FromSeconds(Mock.MockUtil.Random.NextDouble(.7, 20)))
                        .Where(time=>_doContinue)
                        .Select(t =>
                        {
                            var broadcastId = Guid.NewGuid();

                            const int minSliverCount = 1;
                            const int maxSliverCount = 200;
                            var sliverCount = Mock.MockUtil.Random.Next(minSliverCount, maxSliverCount);
                            const int sc = maxSliverCount - minSliverCount;
                            const int oneBytePercent = sc / 10;
                            const int scMid = sc / 2;
                            const int scMidOffset = scMid + oneBytePercent;
                            sliverCount = sliverCount > scMid && sliverCount < scMidOffset ? 1 : sliverCount;
                            var slivers = Observable
                                .Range(1, sliverCount)
                                .Select(sliverIndex =>
                                {
                                    var successNum = Mock.MockUtil.Random.Next(1, 101);
                                    var success = successNum == 1 ? true : (bool?)null;

                                    if (!success.HasValue)
                                    {
                                        var successNum2 = Mock.MockUtil.Random.Next(1, 101);
                                        var success2 = successNum2 < 90;
                                        Observable
                                            .Timer(TimeSpan.FromSeconds(Mock.MockUtil.Random.NextDouble(.7, 20)))
                                            .Where(l => _doContinue)
                                            .Subscribe(l =>
                                            {
                                                _sliverChanged
                                                    .OnNext(new SliverChangedParams()
                                                    {
                                                        BroadcastId = broadcastId,
                                                        SliverIndex = (uint)sliverIndex,
                                                        Success = success2
                                                    });
                                            });
                                    }

                                    return new SliverViewmodelParams(dispatcher)
                                    {
                                        BroadcastId = broadcastId,
                                        SliverIndex = (uint)sliverIndex,
                                        Success = success,
                                        
                                    };
                                });

                            var param = new BroadcastViewmodelParams()
                            {
                                BroadcastId = broadcastId,
                                Slivers = slivers.ToEnumerable().ToArray()
                            };
                            return param;
                        });

                })
                .Merge();
        }


    }
}