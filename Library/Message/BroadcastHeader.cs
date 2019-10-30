using System;

namespace Library.Message
{
    public class BroadcastHeader : MessageHeader, IBroadcastHeader
    {
        public BroadcastHeader(Guid broadcastId, bool? isLast, long chunkSizeInBytes) : base(broadcastId, isLast)
        {
            MaxPayloadSizeInBytes = chunkSizeInBytes;
        }

        public long MaxPayloadSizeInBytes { get; }
    }
}