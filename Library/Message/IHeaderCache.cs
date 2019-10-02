using System;

namespace Library.Message
{
    public interface IHeaderCache
    {
        void Handle(IObservable<IBroadcastHeader> observable);
        void Handle(IObservable<IChunkHeader> observable);
        void Handle(IObservable<IFileHeader> observable);
        IObservable<IFileReadyMessage> FileReadyObservable { get; }
    }
}