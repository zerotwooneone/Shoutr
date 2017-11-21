using System.Collections.ObjectModel;
using ShoutrGui.Listen;

namespace ShoutrGui
{
    public interface IMainWindowViewmodel
    {
        ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}