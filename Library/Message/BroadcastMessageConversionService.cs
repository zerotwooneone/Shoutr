using System;
using System.IO;
using System.Numerics;
using ProtoBuf;

namespace Library.Message
{
    public class BroadcastMessageConversionService : IBroadcastMessageConversionService
    {
        public static byte[] Convert(ProtoMessage protoMessage)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, protoMessage);
                return stream.ToArray();
            }
        }

        public byte[] Convert(IBroadcastHeader broadcastHeader)
        {
            var protoMessage = ConvertToProtoMessage(broadcastHeader);
            return Convert(protoMessage);
        }

        public static ProtoMessage ConvertToProtoMessage(IBroadcastHeader broadcastHeader)
        {
            return new ProtoMessage
            (
                broadcastId: broadcastHeader.BroadcastId,
                chunkSizeInBytes: broadcastHeader.ChunkSizeInBytes,
                isLast: broadcastHeader.IsLast
            );
        }

        public byte[] Convert(IFileHeader fileHeader)
        {
            var protoMessage = ConvertToProtoMessage(fileHeader);
            return Convert(protoMessage);
        }

        public static ProtoMessage ConvertToProtoMessage(IFileHeader fileHeader)
        {
            return new ProtoMessage
            (
                broadcastId: fileHeader.BroadcastId,
                chunkCount: fileHeader.ChunkCount,
                fileName: fileHeader.FileName,
                isLast: fileHeader.IsLast
            );
        }

        public byte[] Convert(IChunkHeader chunkHeader)
        {
            var protoMessage = ConvertToProtoMessage(chunkHeader);
            return Convert(protoMessage);
        }

        public static ProtoMessage ConvertToProtoMessage(IChunkHeader chunkHeader)
        {
            return new ProtoMessage(chunkHeader.BroadcastId,
                chunkId: chunkHeader.ChunkId,
                isLast: chunkHeader.IsLast);
        }

        public byte[] Convert(IPayloadMessage payloadMessage)
        {
            var protoMessage = ConvertToProtoMessage(payloadMessage);
            return Convert(protoMessage);
        }

        public static ProtoMessage ConvertToProtoMessage(IPayloadMessage payloadMessage)
        {
            return new ProtoMessage(payloadMessage.BroadcastId,
                chunkId: payloadMessage.ChunkId,
                payloadId: payloadMessage.PayloadId,
                payload: payloadMessage.Payload);
        }

        public Messages Convert(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var protoMessage = Serializer.Deserialize<ProtoMessage>(stream);
                var chunkId = protoMessage.GetChunkId();
                var payloadId = protoMessage.GetPayloadId();

                if (payloadId != null)
                {
                    return new Messages
                    {
                        PayloadMessage = new PayloadMessage(protoMessage.BroadcastId,
                            payloadId.Value,
                            protoMessage.Payload,
                            chunkId.Value)
                    };
                }
                if (chunkId != null)
                {
                    return new Messages
                    {
                        ChunkHeader = new ChunkHeader(protoMessage.BroadcastId,
                        chunkId.Value,
                            protoMessage.IsLast)
                    };
                }

                if (protoMessage.FileName != null)
                {
                    var chunkCount = protoMessage.GetChunkCountInBytes();
                    return new Messages
                    {
                        FileHeader = new FileHeader(protoMessage.BroadcastId,
                        protoMessage.IsLast,
                        protoMessage.FileName,
                        chunkCount.Value)
                    };
                }

                return new Messages
                {
                    BroadcastHeader = new BroadcastHeader(protoMessage.BroadcastId,
                    protoMessage.IsLast,
                    protoMessage.ChunkSizeInBytes.Value)
                };
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

            public BigInteger? GetChunkId() => GetNullable(ChunkId);
            public BigInteger? GetPayloadId() => GetNullable(PayloadId);
            public BigInteger? GetChunkCountInBytes() => GetNullable(ChunkCountInBytes);

            /// <summary>
            /// Parameterless constructory required for deserialization
            /// </summary>
            public ProtoMessage()
            {

            }

            public ProtoMessage(Guid broadcastId,
                BigInteger? chunkId = null,
                BigInteger? payloadId = null,
                 byte[] payload = null,
                bool? isLast = null,
                ushort? chunkSizeInBytes = null,
                string fileName = null,
                BigInteger? chunkCount = null)
            {
                BroadcastId = broadcastId;
                ChunkId = chunkId?.ToByteArray();
                PayloadId = payloadId?.ToByteArray();
                Payload = payload;
                IsLast = isLast.HasValue && isLast.Value;
                ChunkSizeInBytes = chunkSizeInBytes;
                FileName = fileName;
                ChunkCountInBytes = chunkCount?.ToByteArray();
            }

            public static BigInteger? GetNullable(byte[] bytes)
            {
                return bytes == null ? (BigInteger?)null : new BigInteger(bytes);
            }
        }

        public byte[] Convert(IBroadcastMessage broadcastMessage)
        {
            var bcId = broadcastMessage.BroadcastId;
            var il = (broadcastMessage as IMessageHeader)?.IsLast;
            var plo = broadcastMessage as IPayloadMessage;
            var pli = plo?.PayloadId;
            var pl = plo?.Payload;

            var protoMessage = new ProtoMessage(bcId, chunkId: null, payloadId: pli, payload: pl, isLast: il,
                chunkSizeInBytes: null, fileName: null, chunkCount: null);
            return Convert(protoMessage);
        }
    }
}