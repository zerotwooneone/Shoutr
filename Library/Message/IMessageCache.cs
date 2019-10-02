using System;

namespace Library.Message
{
    public interface IMessageCache
    {
        void Handle(IObservable<IReceivedMessage> messagesObservable);
        IObservable<ICachedMessage> CachedObservable { get; }
    }
}