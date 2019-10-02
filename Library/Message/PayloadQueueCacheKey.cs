using System;
using System.Numerics;

namespace Library.Message
{
    public class PayloadQueueCacheKey
    {
        public PayloadQueueCacheKey(Guid broadCastId, 
            BigInteger chunkIndex)
        {
            BroadCastId = broadCastId;
            ChunkIndex = chunkIndex;
        }

        public Guid BroadCastId { get; }
        public BigInteger ChunkIndex { get; }

        public override bool Equals(object other)
        {
            var x = other as PayloadQueueCacheKey;
            if (x == null) return false;
            return x.BroadCastId == BroadCastId &&
                   x.ChunkIndex == ChunkIndex;
        }

        public override int GetHashCode()
        {
            const int magicHash1 = 17;
            const int magicHash2 = 31;
            int hash = magicHash1;
            hash = hash * magicHash2 + BroadCastId.GetHashCode();
            hash = hash * magicHash2 + ChunkIndex.GetHashCode();
            return hash;
        }
    }
}