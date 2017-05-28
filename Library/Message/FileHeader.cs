using System;
using System.Numerics;

namespace Library.Message
{
    public class FileHeader : MessageHeader, IFileHeader
    {
        public FileHeader(Guid broadcastId, bool? isLast, string fileName, BigInteger chunkCount) : base(broadcastId, isLast)
        {
            FileName = fileName;
            ChunkCount = chunkCount;
        }

        public string FileName { get; }
        public BigInteger ChunkCount { get; }

        public override int GetHashCode()
        {
            return BroadcastMessage.GetHashCode((string) FileName, (int) base.GetHashCode());
        }
    }
}