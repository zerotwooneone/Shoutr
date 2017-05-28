using System;

namespace Library.Message
{
    public class BroadcastHeader : MessageHeader, IBroadcastHeader
    {
        public BroadcastHeader(Guid broadcastId, bool? isLast, ushort chunkSizeInBytes) : base(broadcastId, isLast)
        {
            ChunkSizeInBytes = chunkSizeInBytes;
        }

        public ushort ChunkSizeInBytes { get; }
    }
}