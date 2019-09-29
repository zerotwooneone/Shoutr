using System;
using System.Numerics;

namespace Library.Message
{
    public interface ICachedMessage
    {
        Guid BroadcastId { get; }
        BigInteger? PayloadIndex { get; }
        byte[] Payload { get; }
        string FileName { get; }
        BigInteger? ChunkCount { get; }
        BigInteger? ChunkIndex { get; }
    }
}