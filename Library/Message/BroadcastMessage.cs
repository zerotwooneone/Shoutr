using System;
using System.Linq;
using System.Numerics;
using ProtoBuf;

namespace Library.Message
{
    [ProtoContract]
    public class BroadcastMessage
    {
        [ProtoMember(1)]
        public Guid BroadcastId { get; set; }
        
        public BigInteger? ChunkId { get; set; }
        public BigInteger? PayloadId { get; set; }
        [ProtoMember(4)]
        public byte[] Payload { get; set; }
        [ProtoMember(5)]
        public bool? IsLast { get; set; }
        [ProtoMember(6)]
        public ushort? ChunkSizeInBytes { get; set; }
        [ProtoMember(7)]
        public string FileName { get; set; }
        public BigInteger? ChunkCount { get; set; }

        [ProtoMember(2)]
        public byte[] ChunkIdBytes { get => ChunkId?.ToByteArray();
            set => ChunkId = value == null ? (BigInteger?) null : new BigInteger(value);
        }

        [ProtoMember(3)]
        public byte[] PayloadIdBytes
        {
            get => PayloadId?.ToByteArray();
            set => PayloadId = value == null ? (BigInteger?)null : new BigInteger(value);
        }

        [ProtoMember(8)]
        public byte[] ChunkCountBytes
        {
            get => ChunkCount?.ToByteArray();
            set => ChunkCount = value == null ? (BigInteger?)null : new BigInteger(value);
        }

        public override bool Equals(object obj)
        {
            var other = obj as BroadcastMessage;
            return Equals(other);
        }

        public bool Equals(BroadcastMessage other)
        {
            if (other == null)
            {
                return false;
            }
            return BroadcastId == other.BroadcastId
                && ChunkId == other.ChunkId
                && PayloadId == other.PayloadId
                && ((Payload == null && other.Payload == null) || (Payload != null && Payload.SequenceEqual(other.Payload)))
                && IsLast == other.IsLast
                && ChunkSizeInBytes == other.ChunkSizeInBytes
                && FileName == other.FileName
                && ChunkCount == other.ChunkCount;
        }

        public override int GetHashCode()
        {
            unchecked // disable overflow, for the unlikely possibility that you
            {         // are compiling with overflow-checking enabled
                BigInteger hash = 27;
                hash = (13 * hash) + new BigInteger(BroadcastId.ToByteArray());
                hash = OptionalHash(ChunkId, hash);
                hash = OptionalHash(PayloadId, hash);

                return (int)(hash % int.MaxValue);
            }
        }

        private static BigInteger OptionalHash(BigInteger? bigInt, BigInteger hash)
        {
            if (bigInt.HasValue)
            {
                hash = (13 * hash) + bigInt.Value;
            }
            return hash;
        }
    }
}