using System;

namespace Library.Interface.Message
{
    public interface IMessageCache
    {
        void Handle(IObservable<IReceivedMessage> messagesObservable);
        IObservable<IFileWriteRequest> FileWriteRequestObservable { get; }
    }
}