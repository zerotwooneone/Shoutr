using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfPractice.Annotations;
using WpfPractice.BroadcastSliver;

namespace WpfPractice.Mock
{
    public class MockBroadcastSliverViewmodel : IBroadcastSliverViewmodel
    {
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

        public MockBroadcastSliverViewmodel()
        {
            Color = BroadcastSliverViewmodel.Initial;
            Task
                .Delay(TimeSpan.FromSeconds(MockUtil.Random.NextDouble(.3, 1)))
                .ContinueWith(t =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Color = BroadcastSliverViewmodel.Complete;
                    });
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}