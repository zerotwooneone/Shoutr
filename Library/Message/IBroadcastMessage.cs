using System;

namespace Library.Message
{
    public interface IBroadcastMessage
    {
        Guid BroadcastId { get; }
    }
}