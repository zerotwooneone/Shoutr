using System;
using System.Windows.Media;
using WpfPractice.DataModel;
using WpfPractice.Viewmodel;

namespace WpfPractice.BroadcastSliver
{
    public class BroadcastSliverViewmodel : ViewmodelBase, IBroadcastSliverViewmodel
    {
        public Guid BroadcastId { get; }
        public uint SliverIndex { get; }
        private readonly IBroadcastSliverService _broadcastSliverService;
        private readonly IObservable<SliverChangedParams> _changed;
        private Brush _color;

        public Brush Color
        {
            get { return _color; }
            private set
            {
                if (Equals(value, _color)) return;
                _color = value;
                OnPropertyChanged();
            }
        }

        public static readonly Brush Initial = new SolidColorBrush(Colors.DarkSlateBlue);
        public static readonly Brush Complete = new SolidColorBrush(Colors.MediumSeaGreen);
        public static readonly Brush Failed = new SolidColorBrush(Colors.IndianRed);

        static BroadcastSliverViewmodel()
        {
            Initial.Freeze();
            Complete.Freeze();
            Failed.Freeze();
        }

        public BroadcastSliverViewmodel(IBroadcastSliverService broadcastSliverService,
            SliverViewmodelParams sliverViewmodelParams,
            IObservable<SliverChangedParams> changed)
        {
            BroadcastId = sliverViewmodelParams.BroadcastId;
            SliverIndex = sliverViewmodelParams.SliverIndex;
            _broadcastSliverService = broadcastSliverService;
            _changed = changed;
            _changed
                .Subscribe(param =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Color = GetSuccessColor(param.Success);
                    });
                });
            Color = GetSuccessColor(sliverViewmodelParams.Success);
        }

        private Brush GetSuccessColor(bool? isSuccess)
        {
            return isSuccess.HasValue ?
                isSuccess.Value ?
                    Complete : Failed : Initial;
        }
    }
}
