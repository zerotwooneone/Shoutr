using System;
using System.Numerics;

namespace Library.Message
{
    public class ChunkHeader : MessageHeader, IChunkHeader
    {
        public ChunkHeader(Guid broadcastId, 
            BigInteger chunkId, 
            bool? isLast = null) : base(broadcastId, isLast)
        {
            ChunkId = chunkId;
        }

        public BigInteger ChunkId { get; }
        

        public override int GetHashCode()
        {
            return GetHashCode(ChunkId, base.GetHashCode());
        }
    }
}