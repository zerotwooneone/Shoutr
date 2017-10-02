using System.ComponentModel;
using System.Windows.Media;

namespace WpfPractice.BroadcastSliver
{
    public interface IBroadcastSliverViewmodel: INotifyPropertyChanged
    {
        Brush Color { get; }
    }
}