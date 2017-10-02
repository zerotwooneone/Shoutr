using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfPractice.BroadcastSliver
{
    public class BroadcastSliverViewmodel: IBroadcastSliverViewmodel
    {
        private readonly IBroadcastSliverService _broadcastSliverService;
        public Brush Color { get; }
        public static readonly Brush Initial = new SolidColorBrush(Colors.DarkSlateBlue);
        public BroadcastSliverViewmodel(IBroadcastSliverService broadcastSliverService,
            uint sliverIndex)
        {
            _broadcastSliverService = broadcastSliverService;
            //_broadcastSliverService.OnSilverChanged += OnSliverChanged;
            Color = Initial;
        }
    }

    public interface IBroadcastSliverService
    {

    }

    
}
