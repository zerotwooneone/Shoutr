using System;

namespace WpfPractice.Listen
{
    public class ListenService : IListenService
    {
        public ListenService()
        {
        
            Extensions.Repeat(() =>
            {
                OnNewBroadcast(Guid.NewGuid());
            }, 1000, .7, 4);
        }

        public event EventHandler<Guid> NewBroadcast;

        protected virtual void OnNewBroadcast(Guid broadcastId)
        {
            NewBroadcast?.Invoke(this, broadcastId);
        }
    }
}