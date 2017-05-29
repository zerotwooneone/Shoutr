using System.Numerics;

namespace Library.Message
{
    public interface IPayloadMessage : IBroadcastMessage
    {
        BigInteger ChunkIndex { get; }
        BigInteger PayloadIndex { get; }
        byte[] Payload { get; }
    }
}