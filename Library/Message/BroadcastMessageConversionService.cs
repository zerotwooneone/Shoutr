using System;
using System.IO;
using System.Numerics;
using Library.File;
using ProtoBuf;

namespace Library.Message
{
    public class BroadcastMessageConversionService : IBroadcastMessageConversionService
    {
        public byte[] Convert(BroadcastMessage broadcastMessage)
        {
            var protoMessage = new ProtoMessage(broadcastMessage);
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, protoMessage);
                return stream.ToArray();
            }
        }

        public BroadcastMessage Convert(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<ProtoMessage>(stream).ToBroadcastMessage();
            }
        }

        [ProtoContract]
        public class ProtoMessage
        {
            [ProtoMember(1)]
            public Guid BroadcastId { get; set; }
            [ProtoMember(2)]
            public byte[] ChunkId { get; set; }
            [ProtoMember(3)]
            public byte[] PayloadId { get; set; }
            [ProtoMember(4)]
            public byte[] Payload { get; set; }
            [ProtoMember(5)]
            public bool? IsLast { get; set; }
            [ProtoMember(6)]
            public ushort? ChunkSizeInBytes { get; set; }
            [ProtoMember(7)]
            public string FileName { get; set; }
            [ProtoMember(8)]
            public byte[] ChunkCountInBytes { get; set; }

            /// <summary>
            /// Parameterless constructory required for deserialization
            /// </summary>
            public ProtoMessage()
            {

            }

            public ProtoMessage(BroadcastMessage broadcastMessage)
            {
                BroadcastId = broadcastMessage.BroadcastId;
                ChunkId = broadcastMessage.ChunkId?.ToByteArray();
                PayloadId = broadcastMessage.PayloadId?.ToByteArray();
                Payload = broadcastMessage.Payload;
                IsLast = broadcastMessage.IsLast;
                ChunkSizeInBytes = broadcastMessage.ChunkSizeInBytes;
                FileName = broadcastMessage.FileName;
                ChunkCountInBytes = broadcastMessage.ChunkCount?.ToByteArray();
            }

            public BroadcastMessage ToBroadcastMessage()
            {
                return new BroadcastMessage
                {
                    BroadcastId = BroadcastId,
                    ChunkCount = ChunkCountInBytes == null ? (BigInteger?)null : new BigInteger(ChunkCountInBytes),
                    ChunkId = ChunkId == null ? (BigInteger?)null : new BigInteger(ChunkId),
                    ChunkSizeInBytes = ChunkSizeInBytes,
                    FileName = FileName,
                    IsLast = IsLast,
                    Payload = Payload,
                    PayloadId = PayloadId == null ? (BigInteger?)null : new BigInteger(PayloadId)
                };
            }
        }
    }
}