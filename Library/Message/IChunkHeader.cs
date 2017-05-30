using System.Numerics;

namespace Library.Message
{
    /// <summary>
    /// Represents meta data which is general to one whole chunk of data
    /// </summary>
    public interface IChunkHeader : IBroadcastMessage, IMessageHeader
    {
        BigInteger ChunkIndex { get; }
    }
}