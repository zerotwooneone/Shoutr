using System;

namespace Library.Message
{
    public class BroadcastCacheKey
    {
        public BroadcastCacheKey(Guid broadCastId)
        {
            BroadCastId = broadCastId;
        }

        public Guid BroadCastId { get; }

        public override bool Equals(object other)
        {
            var x = other as BroadcastCacheKey;
            if (x == null) return false;
            return x.BroadCastId == BroadCastId;
        }

        public override int GetHashCode()
        {
            return BroadCastId.GetHashCode();
        }
    }
}