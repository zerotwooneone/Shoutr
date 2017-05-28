using System;
using System.Numerics;

namespace Library.Message
{
    public class PayloadMessage : BroadcastMessage, IPayloadMessage
    {
        public PayloadMessage(Guid broadcastId, BigInteger payloadId, byte[] payload, BigInteger chunkId) : base(broadcastId)
        {
            PayloadId = payloadId;
            Payload = payload;
            ChunkId = chunkId;
        }

        public BigInteger ChunkId { get; }
        public BigInteger PayloadId { get; }
        public byte[] Payload { get; }

        public override int GetHashCode()
        {
            return GetHashCode(PayloadId, GetHashCode(ChunkId, base.GetHashCode()));
        }
    }
}