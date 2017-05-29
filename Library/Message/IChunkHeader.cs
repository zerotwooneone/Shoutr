using System.Numerics;

namespace Library.Message
{
    public interface IChunkHeader : IBroadcastMessage, IMessageHeader
    {
        BigInteger ChunkIndex { get; }
    }
}