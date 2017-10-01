﻿using System;
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

            int count = 0;
                Func<Task> c=null;
            c= ()=> Task
                    .Delay(TimeSpan.FromSeconds(2))
                    .ContinueWith(t => Application.Current.Dispatcher.Invoke(() => Broadcasts.Add(broadcastViewmodelFactory())))
                    .ContinueWith(t =>
                {
                    if (count < 1000) c();
                });
            c();

        }

        public ObservableCollection<IBroadcastViewmodel> Broadcasts { get; }
    }
}