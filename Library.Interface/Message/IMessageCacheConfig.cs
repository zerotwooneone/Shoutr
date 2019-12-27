using System;

namespace Library.Interface.Message
{
    public interface IMessageCacheConfig
    {
        TimeSpan BroadcastCacheExpiration { get; }
        TimeSpan ChunkPayloadCacheExpiration { get; }
    }
}