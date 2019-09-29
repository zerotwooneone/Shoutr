using System;

namespace Library.Message
{
    public interface IMessageCacheConfig
    {
        TimeSpan BroadcastCacheExpiration { get; }
    }
}