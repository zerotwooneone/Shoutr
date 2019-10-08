using System;

namespace Library.Message
{
    public interface IHeaderCache
    {
        void HandleBroadcastHeader(IObservable<IBroadcastHeader> observable);
        void HandleChunkHeader(IObservable<IChunkHeader> observable);
        void HandleFileHeader(IObservable<IFileHeader> observable);
        IObservable<IFileReadyMessage> FileReadyObservable { get; }
    }
}