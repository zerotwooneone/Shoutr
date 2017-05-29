using System;
using System.Numerics;

namespace Library.Message
{
    public class ChunkHeader : MessageHeader, IChunkHeader
    {
        public ChunkHeader(Guid broadcastId, 
            BigInteger chunkIndex, 
            bool? isLast = null) : base(broadcastId, isLast)
        {
            ChunkIndex = chunkIndex;
        }

        public BigInteger ChunkIndex { get; }
        

        public override int GetHashCode()
        {
            return GetHashCode(ChunkIndex, base.GetHashCode());
        }
    }
}