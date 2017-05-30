using System;

namespace Library.Message
{
    /// <summary>
    /// Represents any message capable of being broadcast
    /// </summary>
    public interface IBroadcastMessage
    {
        Guid BroadcastId { get; }
    }
}