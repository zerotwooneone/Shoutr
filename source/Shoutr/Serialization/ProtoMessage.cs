using ProtoBuf;
using System;

namespace Shoutr.Serialization
{
    /// <summary>
    /// This message class is specific to the serialization library being used in this service, ProtocolBuffers. This represents the shape of the data that is actually sent as bytes over the network.
    /// </summary>
    [ProtoContract]
    internal class ProtoMessage
    {
        [ProtoMember(1)]
        public byte[] BroadcastId { get; set; }
        [ProtoMember(2)]
        public long? PayloadIndex { get; set; }
        [ProtoMember(3)]
        public byte[] Payload { get; set; }
        [ProtoMember(4)]
        public long? PayloadMaxSize { get; set; }
        [ProtoMember(5)]
        public string FileName { get; set; }
        [ProtoMember(6)]
        public long? PayloadCount { get; set; }
        [ProtoMember(7)]
        public byte[] Hash { get; set; }
            
        /// <summary>
        /// Parameterless constructor required for protocol buffer deserialization
        /// </summary>
        internal ProtoMessage() {}

        internal ProtoMessage(Guid broadcastId, long? payloadIndex, byte[] payload, long? payloadMaxSize,
            string fileName, long? payloadCount, byte[] hash)
        {
            BroadcastId = broadcastId.ToByteArray();
            PayloadIndex = payloadIndex;
            Payload = payload;
            PayloadMaxSize = payloadMaxSize;
            FileName = fileName;
            PayloadCount = payloadCount;
            Hash = hash;
        }

        internal Guid GetBroadcastId()
        {
            return new Guid(BroadcastId);
        }

        internal string GetHashString()
        {
            if (Hash == null)
            {
                return null;
            }
            return Convert.ToBase64String(Hash);
        }
    }
}
