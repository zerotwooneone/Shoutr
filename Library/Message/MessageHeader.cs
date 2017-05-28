using System;

namespace Library.Message
{
    public abstract class MessageHeader : BroadcastMessage, IMessageHeader
    {
        protected MessageHeader(Guid broadcastId, bool? isLast) : base(broadcastId)
        {
            IsLast = isLast;
        }

        public bool? IsLast { get; }
    }
}