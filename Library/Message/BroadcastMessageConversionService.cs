using System;
using System.IO;
using System.Numerics;
using Library.ByteTransfer;
using ProtoBuf;

namespace Library.Message
{
    public class BroadcastMessageConversionService : IBroadcastMessageConversionService
    {
        private readonly IByteService _byteService;

        public BroadcastMessageConversionService(IByteService byteService)
        {
            _byteService = byteService;
        }

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

        public ProtoMessage ConvertToProtoMessage(IFileHeader fileHeader)
        {
            return new ProtoMessage
            (
                broadcastId: fileHeader.BroadcastId,
                chunkCount: _byteService.GetBytes(fileHeader.ChunkCount),
                fileName: fileHeader.FileName,
                isLast: fileHeader.IsLast
            );
        }

        public byte[] Convert(IChunkHeader chunkHeader)
        {
            var protoMessage = ConvertToProtoMessage(chunkHeader);
            return Convert(protoMessage);
        }

        public ProtoMessage ConvertToProtoMessage(IChunkHeader chunkHeader)
        {
            return new ProtoMessage(chunkHeader.BroadcastId,
                chunkIndex: _byteService.GetBytes(chunkHeader.ChunkIndex),
                isLast: chunkHeader.IsLast);
        }

        public byte[] Convert(IPayloadMessage payloadMessage)
        {
            var protoMessage = ConvertToProtoMessage(payloadMessage);
            return Convert(protoMessage);
        }

        public ProtoMessage ConvertToProtoMessage(IPayloadMessage payloadMessage)
        {
            return new ProtoMessage(payloadMessage.BroadcastId,
                chunkIndex: _byteService.GetBytes(payloadMessage.ChunkIndex),
                payloadIndex: _byteService.GetBytes(payloadMessage.PayloadIndex),
                payload: payloadMessage.Payload);
        }

        public Messages Convert(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var protoMessage = Serializer.Deserialize<ProtoMessage>(stream);
                var chunkIndex = protoMessage.GetChunkIndex();
                var payloadIndex = protoMessage.GetPayloadIndex();

                if (payloadIndex != null)
                {
                    return new Messages
                    {
                        PayloadMessage = new PayloadMessage(protoMessage.BroadcastId,
                            payloadIndex.Value,
                            protoMessage.Payload,
                            chunkIndex.Value)
                    };
                }
                if (chunkIndex != null)
                {
                    return new Messages
                    {
                        ChunkHeader = new ChunkHeader(protoMessage.BroadcastId,
                        chunkIndex.Value,
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

        /// <summary>
        /// This message class is specific to the serialization library being used in this service, ProtocolBuffers. This represents the shape of the data that is actually sent as bytes over the network.
        /// </summary>
        [ProtoContract]
        public class ProtoMessage
        {
            [ProtoMember(1)]
            public Guid BroadcastId { get; set; }
            [ProtoMember(2)]
            public byte[] ChunkIndex { get; set; }
            [ProtoMember(3)]
            public byte[] PayloadIndex { get; set; }
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

            public BigInteger? GetChunkIndex() => GetNullable(ChunkIndex);
            public BigInteger? GetPayloadIndex() => GetNullable(PayloadIndex);
            public BigInteger? GetChunkCountInBytes() => GetNullable(ChunkCountInBytes);

            /// <summary>
            /// Parameterless constructory required for deserialization
            /// </summary>
            public ProtoMessage()
            {

            }

            public ProtoMessage(Guid broadcastId,
                byte[] chunkIndex = null,
                byte[] payloadIndex = null,
                 byte[] payload = null,
                bool? isLast = null,
                ushort? chunkSizeInBytes = null,
                string fileName = null,
                byte[] chunkCount = null)
            {
                BroadcastId = broadcastId;
                ChunkIndex = chunkIndex;
                PayloadIndex = payloadIndex;
                Payload = payload;
                IsLast = isLast.HasValue && isLast.Value;
                ChunkSizeInBytes = chunkSizeInBytes;
                FileName = fileName;
                ChunkCountInBytes = chunkCount;
            }

            public static BigInteger? GetNullable(byte[] bytes)
            {
                return bytes == null ? (BigInteger?)null : new BigInteger(bytes);
            }
        }

        public byte[] Convert(IBroadcastMessage broadcastMessage)
        {
            var broadcastId = broadcastMessage.BroadcastId;
            var isLast = (broadcastMessage as IMessageHeader)?.IsLast;
            var payloadMessage = broadcastMessage as IPayloadMessage;
            var payloadIndex = payloadMessage?.PayloadIndex;
            var payload = payloadMessage?.Payload;

            var protoMessage = new ProtoMessage(broadcastId, 
                chunkIndex: null, 
                payloadIndex: _byteService.GetBytes(payloadIndex), 
                payload: payload, 
                isLast: isLast,
                chunkSizeInBytes: null, fileName: null, chunkCount: null);
            return Convert(protoMessage);
        }
    }
}