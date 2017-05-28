using System.Numerics;

namespace Library.Message
{
    public interface IPayloadMessage : IBroadcastMessage
    {
        BigInteger ChunkId { get; }
        BigInteger PayloadId { get; }
        byte[] Payload { get; }
    }
}