using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfPractice.Annotations;
using WpfPractice.DataModel;

namespace WpfPractice.BroadcastSliver
{
    public class BroadcastSliverViewmodel: IBroadcastSliverViewmodel, INotifyPropertyChanged
    {
        public Guid BroadcastId { get; }
        public uint SliverIndex { get; }
        private readonly IBroadcastSliverService _broadcastSliverService;
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
        public BroadcastSliverViewmodel(IBroadcastSliverService broadcastSliverService,
            Guid broadcastId,
            uint sliverIndex)
        {
            BroadcastId = broadcastId;
            SliverIndex = sliverIndex;
            _broadcastSliverService = broadcastSliverService;
            _broadcastSliverService.BroadcastSliverChanged += OnSliverChanged;
            Color = Initial;
        }

        private void OnSliverChanged(object sender, BroadcastSliverEventArgs e)
        {
            if (e.BroadcastId == BroadcastId &&
                e.SliverIndex == SliverIndex)
            {
                Color = Complete; //temp hack
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface IBroadcastSliverService
    {
        event EventHandler<BroadcastSliverEventArgs> BroadcastSliverChanged;
    }

    
}
