using System;

namespace Library.Message
{
    public interface IPayloadCache
    {
        void Handle(IObservable<IFileReadyMessage> observable);
        void Handle(IObservable<IPayloadMessage> observable);
        IObservable<ICachedMessage> CachedObservable { get; }
    }
}