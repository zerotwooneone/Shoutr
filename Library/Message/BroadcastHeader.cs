using System;

namespace Library.Message
{
    public class BroadcastHeader : MessageHeader, IBroadcastHeader
    {
        public BroadcastHeader(Guid broadcastId, bool? isLast, long maxPayloadSizeInBytes) : base(broadcastId, isLast)
        {
            MaxPayloadSizeInBytes = maxPayloadSizeInBytes;
        }

        public long MaxPayloadSizeInBytes { get; }
    }
}