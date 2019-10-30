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
                maxPayloadSizeInBytes: broadcastHeader.MaxPayloadSizeInBytes,
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
                var chunkIndex = _byteService.GetNullableBigInteger(protoMessage.ChunkIndex);
                var payloadIndex = _byteService.GetNullableBigInteger(protoMessage.PayloadIndex);

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
                    var chunkCount = _byteService.GetBigInteger(protoMessage.ChunkCountInBytes);
                    return new Messages
                    {
                        FileHeader = new FileHeader(protoMessage.BroadcastId,
                        protoMessage.IsLast,
                        protoMessage.FileName,
                        chunkCount)
                    };
                }

                return new Messages
                {
                    BroadcastHeader = new BroadcastHeader(protoMessage.BroadcastId,
                    protoMessage.IsLast,
                    protoMessage.MaxPayloadSizeInBytes.Value)
                };
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
                maxPayloadSizeInBytes: null, fileName: null, chunkCount: null);
            return Convert(protoMessage);
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
            public long? MaxPayloadSizeInBytes { get; set; }
            [ProtoMember(7)]
            public string FileName { get; set; }
            [ProtoMember(8)]
            public byte[] ChunkCountInBytes { get; set; }
            
            /// <summary>
            /// Parameterless constructor required for protocol buffer deserialization
            /// </summary>
            public ProtoMessage()
            {

            }

            public ProtoMessage(Guid broadcastId,
                byte[] chunkIndex = null,
                byte[] payloadIndex = null,
                 byte[] payload = null,
                bool? isLast = null,
                long? maxPayloadSizeInBytes = null,
                string fileName = null,
                byte[] chunkCount = null)
            {
                BroadcastId = broadcastId;
                ChunkIndex = chunkIndex;
                PayloadIndex = payloadIndex;
                Payload = payload;
                IsLast = isLast.HasValue && isLast.Value;
                MaxPayloadSizeInBytes = maxPayloadSizeInBytes;
                FileName = fileName;
                ChunkCountInBytes = chunkCount;
            }
        }
    }
}