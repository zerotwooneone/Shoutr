using System;
using System.Numerics;

namespace Library.Interface.Message
{
    public interface IHeaderCache
    {
        void HandleBroadcastHeader(IObservable<IBroadcastHeader> observable);
        void HandleChunkHeader(IObservable<IChunkHeader> observable);
        void HandleFileHeader(IObservable<IFileHeader> observable);
        IObservable<IFileReadyMessage> FileReadyObservable { get; }
        string GetFileName(Guid broadcastId, BigInteger chunkIndex, BigInteger payloadIndex);
    }
}