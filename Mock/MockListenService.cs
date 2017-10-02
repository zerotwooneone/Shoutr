using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows;
using WpfPractice.Listen;

namespace WpfPractice.Mock
{
    public class MockListenService:IListenService
    {
        public MockListenService()
        {
            Extensions.Repeat(() =>
            {
                OnNewBroadcast();
            },1000,.7,4);
        }
        public Guid GetNextBroadcastId()
        {
            return Guid.NewGuid();
        }

        public event EventHandler NewBroadcast;

        protected virtual void OnNewBroadcast()
        {
            NewBroadcast?.Invoke(this, EventArgs.Empty);
        }
    }
}