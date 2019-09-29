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
            //const int magicHash1 = 17;
            //const int magicHash2 = 31;
            //int hash = magicHash1;
            //hash = hash * magicHash2 + BroadCastId.GetHashCode();
            return BroadCastId.GetHashCode();
        }
    }
}