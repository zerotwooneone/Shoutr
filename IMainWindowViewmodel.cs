using System.Collections.ObjectModel;
using WpfPractice.Listen;

namespace WpfPractice
{
    public interface IMainWindowViewmodel
    {
        ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}