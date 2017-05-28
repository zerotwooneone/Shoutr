using System.Numerics;

namespace Library.Message
{
    public interface IBroadcastHeader : IBroadcastMessage, IMessageHeader
    {
        ushort ChunkSizeInBytes { get; }
    }
}