using System.Numerics;

namespace Library.Message
{
    public interface IFileHeader : IBroadcastMessage, IMessageHeader
    {
        string FileName { get; }
        BigInteger ChunkCount { get; }
    }
}