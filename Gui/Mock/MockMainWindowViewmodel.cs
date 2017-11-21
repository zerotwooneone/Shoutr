using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ShoutrGui.Listen;
using ShoutrGui.Viewmodel;

namespace ShoutrGui.Mock
{
    public class MockMainWindowViewmodel : ViewmodelBase, IMainWindowViewmodel
    {
        public MockMainWindowViewmodel()
        {
            Broadcasts = new ObservableCollection<IBroadcastViewmodel>();
            Extensions.Repeat(() =>
            {
                var viewmodel = new MockListenBroadcastViewmodel();
                Broadcasts.Add(viewmodel);
                Task
                    .Delay(TimeSpan.FromSeconds(.7))
                    .ContinueWith(t => Broadcasts.Remove(viewmodel));
            }, 1000, .7, 5);
        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}