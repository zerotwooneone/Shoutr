using System;
using System.Numerics;

namespace Library.Interface.Message
{
    public class PayloadMessage : BroadcastMessage, IPayloadMessage
    {
        public PayloadMessage(Guid broadcastId, BigInteger payloadIndex, byte[] payload, BigInteger chunkIndex) : base(broadcastId)
        {
            PayloadIndex = payloadIndex;
            Payload = payload;
            ChunkIndex = chunkIndex;
        }

        public BigInteger ChunkIndex { get; }
        public BigInteger PayloadIndex { get; }
        public byte[] Payload { get; }

        public override int GetHashCode()
        {
            return GetHashCode(PayloadIndex, GetHashCode(ChunkIndex, base.GetHashCode()));
        }
    }
}