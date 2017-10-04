using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using WpfPractice.BroadcastSliver;
using WpfPractice.DataModel;

namespace WpfPractice.Listen
{
    public class ListenService : IListenService
    {
        public IObservable<BroadcastViewmodelParams> NewBroadcast => _newBroadcast;
        private readonly Subject<BroadcastViewmodelParams> _newBroadcast;
        private readonly Subject<SliverChangedParams> _sliverChanged;
        public IObservable<SliverChangedParams> SliverChanged => _sliverChanged;

        public ListenService()
        {
            _newBroadcast = new Subject<BroadcastViewmodelParams>();
            _sliverChanged = new Subject<SliverChangedParams>();
            Extensions.Repeat(() =>
            {
                var broadcastId = Guid.NewGuid();

                var sliverCount = Mock.MockUtil.Random.Next(1, 1000);
                sliverCount = sliverCount > 100 && sliverCount < 200 ? 1 : sliverCount;
                var slivers = Enumerable
                .Range(0, sliverCount)
                .Select(i =>
                    {
                        var successNum = Mock.MockUtil.Random.Next(1, 101);
                        var success = successNum == 1 ? true : (bool?)null;

                        if (!success.HasValue)
                        {
                            var successNum2 = Mock.MockUtil.Random.Next(1, 101);
                            var success2 = successNum2 < 90;
                            Observable
                                .Timer(TimeSpan.FromSeconds(Mock.MockUtil.Random.NextDouble(.7, 2)))
                                .Subscribe(l =>
                                {
                                    _sliverChanged
                                        .OnNext(new SliverChangedParams()
                                        {
                                            BroadcastId = broadcastId,
                                            SliverIndex = (uint)i,
                                            Success = success2
                                        });
                                });
                        }

                        return new SliverViewmodelParams()
                        {
                            BroadcastId = broadcastId,
                            SliverIndex = (uint)i,
                            Success = success
                        };


                    });

                var param = new BroadcastViewmodelParams()
                {
                    BroadcastId = broadcastId,
                    Slivers = slivers
                };
                _newBroadcast.OnNext(param);
            }, 1000, .7, 4);
        }


    }
}