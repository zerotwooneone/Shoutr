using System;
using System.Numerics;

namespace Library.Message
{
    public class CachedMessage : ICachedMessage
    {
        public CachedMessage(Guid broadcastId, 
            BigInteger? chunkIndex, 
            BigInteger? payloadIndex, 
            byte[] payload, 
            string fileName, 
            BigInteger? chunkCount,
            bool isLast = false)
        {
            BroadcastId = broadcastId;
            ChunkIndex = chunkIndex;
            PayloadIndex = payloadIndex;
            Payload = payload;
            IsLast = isLast;
            FileName = fileName;
            ChunkCount = chunkCount;
        }

        /// <summary>
        /// This is meant to represent the end of a broadcast when the stream end abnormally
        /// </summary>
        /// <param name="broadcastId"></param>
        /// <param name="fileName"></param>
        public CachedMessage(Guid broadcastId)
        {
            BroadcastId = broadcastId;
            ChunkIndex = 0;
            PayloadIndex = 0;
            Payload = null;
            IsLast = true;
            FileName = null;
            ChunkCount = 0;
        }

        public Guid BroadcastId { get; }
        public BigInteger? ChunkIndex { get; }
        public BigInteger? PayloadIndex { get; }
        public byte[] Payload { get; }
        public bool? IsLast { get; }
        public string FileName { get; }
        public BigInteger? ChunkCount { get; }
    }
}