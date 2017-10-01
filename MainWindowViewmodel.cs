using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfPractice.Listen;

namespace WpfPractice
{
    public class MainWindowViewmodel : IMainWindowViewmodel
    {
        public MainWindowViewmodel(Func<IBroadcastViewmodel> broadcastViewmodelFactory)
        {
            Broadcasts = new ObservableCollection<IBroadcastViewmodel>();
            Task
                .Delay(TimeSpan.FromSeconds(2))
                .ContinueWith(t => Application.Current.Dispatcher.Invoke(() => Broadcasts.Add(broadcastViewmodelFactory())));
        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}