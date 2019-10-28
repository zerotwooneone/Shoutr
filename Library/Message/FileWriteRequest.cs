using System;
using System.Numerics;

namespace Library.Message
{
    public class FileWriteRequest : IFileWriteRequest
    {
        public FileWriteRequest(Guid broadcastId, 
            BigInteger? payloadIndex, 
            byte[] payload, 
            string fileName, 
            bool isLast,
            long maxPayloadSizeInBytes)
        {
            BroadcastId = broadcastId;
            PayloadIndex = payloadIndex;
            Payload = payload;
            IsLast = isLast;
            FileName = fileName;
            MaxPayloadSizeInBytes = maxPayloadSizeInBytes;
        }
        
        public Guid BroadcastId { get; }
        public BigInteger? PayloadIndex { get; }
        public byte[] Payload { get; }
        public bool? IsLast { get; }
        public string FileName { get; }
        public long MaxPayloadSizeInBytes {get;}
    }
}