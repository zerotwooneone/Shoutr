using System;
using System.Numerics;

namespace Library.Interface.Message
{
    public abstract class BroadcastMessage : IBroadcastMessage
    {
        protected BroadcastMessage(Guid broadcastId)
        {
            BroadcastId = broadcastId;
        }

        public Guid BroadcastId { get; }

        public override int GetHashCode()
        {
            return GetHashCode(BroadcastId);
        }

        public static int GetHashCode(Guid guid)
        {
            const int baseHash = 27;
            return GetHashCode(guid.ToByteArray(), baseHash);
        }

        public static int GetHashCode(byte[] bytes, int baseHash)
        {
            var bigInt = new BigInteger(bytes);
            return GetHashCode(bigInt, baseHash);
        }

        public static int GetHashCode(BigInteger bigInt, int baseHash)
        {
            return GetHashCode((int)bigInt,baseHash);
        }

        public static int GetHashCode(string str, int baseHash)
        {
            return GetHashCode(str.GetHashCode(), baseHash);
        }

        public static int GetHashCode(int integer, int baseHash)
        {
            const int hashFactor = 13;
            return (hashFactor * baseHash) + integer;
        }
    }
}