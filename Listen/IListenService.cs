using System;

namespace WpfPractice.Listen
{
    public interface IListenService
    {
        event EventHandler<Guid> NewBroadcast;
    }
}