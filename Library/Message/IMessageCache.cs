using System;

namespace Library.Message
{
    public interface IMessageCache
    {
        void Handle(IObservable<IReceivedMessage> messagesObservable);
        IObservable<IFileWriteRequest> CachedObservable { get; }
    }
}