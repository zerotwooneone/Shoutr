using System;
using System.Numerics;

namespace Library.Message
{
    public interface IPayloadCache
    {
        void HandleFileReady(IObservable<IFileReadyMessage> observable);
        void HandlePayload(IObservable<IPayloadMessage> observable, Func<Guid, BigInteger, BigInteger, string> getFileName);
        IObservable<ICachedMessage> CachedObservable { get; }
    }
}