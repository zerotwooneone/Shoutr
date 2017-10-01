using System;

namespace WpfPractice.Listen
{
    public class ListenService : IListenService
    {
        public Guid GetNextBroadcastId()
        {
            return Guid.NewGuid();
        }
    }
}