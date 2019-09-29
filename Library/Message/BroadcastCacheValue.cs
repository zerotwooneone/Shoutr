using System;

namespace Library.Message
{
    public class BroadcastCacheValue
    {
        public BroadcastCacheValue(Guid broadCastId)
        {
            BroadCastId = broadCastId;
        }

        public Guid BroadCastId { get; }
        public string FileName { get; set; }
    }
}