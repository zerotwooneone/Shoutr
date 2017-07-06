using System;

namespace Library.Message
{
    public class BroadcastHeader : MessageHeader, IBroadcastHeader
    {
        public BroadcastHeader(Guid broadcastId, bool? isLast, long chunkSizeInBytes) : base(broadcastId, isLast)
        {
            ChunkSizeInBytes = chunkSizeInBytes;
        }

        public long ChunkSizeInBytes { get; }
    }
}