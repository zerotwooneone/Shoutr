using System;
using System.Collections.Generic;
using System.Numerics;
using Library.Message;

namespace Library.File
{
    /// <summary>
    /// Converts between the raw bytes from files and the messages for transmission
    /// </summary>
    public interface IFileMessageService
    {
        IBroadcastHeader GetBroadcastHeader(string fileName, Guid broadcastId, 
            IFileMessageConfig fileMessageConfig, bool? isLast = null);
        IFileHeader GetFileHeader(string fileName, Guid broadcastId, bool? isLast = null);
        IChunkHeader GetChunkHeader(string fileName, Guid broadcastId, BigInteger chunkIndex, bool? isLast = null);
        IEnumerable<IPayloadMessage> GetPayloadsByChunkIndex(string fileName, Guid broadcastId, BigInteger chunkIndex);
    }
}