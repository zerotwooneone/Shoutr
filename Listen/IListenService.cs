using System;

namespace WpfPractice.Listen
{
    public interface IListenService
    {
        Guid GetNextBroadcastId();
        event EventHandler NewBroadcast;
    }
}