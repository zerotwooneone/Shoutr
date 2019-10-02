using System;
using System.Numerics;

namespace Library.Message
{
    public interface IFileReadyMessage
    {
        Guid BroadcastId { get; }
        string FileName { get; }
        BigInteger? ChunkIndex { get; }
    }
}