using System;

namespace Library.Interface.Message
{
    public interface IFileReadyMessage
    {
        Guid BroadcastId { get; }
        string FileName { get; }
        long ChunkSizeInBytes { get; }
    }
}