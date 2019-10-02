using System;
using System.Numerics;

namespace Library.Message
{
    public class FileReadyMessage : IFileReadyMessage
    {
        public FileReadyMessage(Guid broadcastId, 
            string fileName, 
            BigInteger? chunkIndex)
        {
            BroadcastId = broadcastId;
            FileName = fileName;
            ChunkIndex = chunkIndex;
        }

        public Guid BroadcastId { get; }
        public string FileName { get; }
        public BigInteger? ChunkIndex { get; }
    }
}