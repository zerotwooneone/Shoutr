using System;
using System.Collections.Generic;
using System.Numerics;
using Library.Message;

namespace Library.File
{
    public interface IFileMessageService
    {
        IBroadcastHeader GetBroadcastHeader(string fileName, Guid broadcastId);
        IFileHeader GetFileHeader(string fileName, Guid broadcastId);
        IChunkHeader GetChunkHeader(string fileName, Guid broadcastId, BigInteger chunkIndex);
        IEnumerable<IPayloadMessage> GetPayloadsByChunkIndex(string fileName, Guid broadcastId, BigInteger chunkIndex);
    }
}