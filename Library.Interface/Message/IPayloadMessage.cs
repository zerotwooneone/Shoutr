using System.Numerics;

namespace Library.Interface.Message
{
    /// <summary>
    /// Represents a message that contains a peice of the data from the source. This message makes up the bulk of the data transmission protocol.
    /// </summary>
    public interface IPayloadMessage : IBroadcastMessage
    {
        BigInteger ChunkIndex { get; }
        BigInteger PayloadIndex { get; }
        byte[] Payload { get; }
    }
}