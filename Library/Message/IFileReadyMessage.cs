using System;

namespace Library.Message
{
    public interface IFileReadyMessage
    {
        Guid BroadcastId { get; }
        string FileName { get; }
        long ChunkSizeInBytes { get; }
    }
}