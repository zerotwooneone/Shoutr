using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WpfPractice.Listen;

namespace WpfPractice.Mock
{
    public class MockMainWindowViewmodel : IMainWindowViewmodel
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
            },1000,.7,5);
        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}