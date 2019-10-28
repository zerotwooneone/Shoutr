using System;
using System.Numerics;

namespace Library.Message
{
    public interface IFileWriteRequest
    {
        Guid BroadcastId { get; }
        BigInteger? PayloadIndex { get; }
        byte[] Payload { get; }
        string FileName { get; }
        long MaxPayloadSizeInBytes {get;}
    }
}