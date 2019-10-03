using System;

namespace Library.Message
{
    public interface IPayloadCache
    {
        void HandleFileReady(IObservable<IFileReadyMessage> observable);
        void HandlePayload(IObservable<IPayloadMessage> observable);
        IObservable<ICachedMessage> CachedObservable { get; }
    }
}