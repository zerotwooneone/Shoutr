using System;

namespace Library.Message
{
    public class FileReadyMessage : IFileReadyMessage
    {
        public FileReadyMessage(Guid broadcastId, 
            string fileName, 
            long chunkSizeInBytes)
        {
            BroadcastId = broadcastId;
            FileName = fileName;
            ChunkSizeInBytes = chunkSizeInBytes;
        }

        public Guid BroadcastId { get; }
        public string FileName { get; }
        public long ChunkSizeInBytes { get; }
    }
}