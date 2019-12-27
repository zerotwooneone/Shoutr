using System;

namespace Library.Interface.Message
{
    /// <summary>
    /// Represents any message capable of being broadcast
    /// </summary>
    public interface IBroadcastMessage
    {
        Guid BroadcastId { get; }
    }
}