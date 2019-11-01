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
        IFileHeader GetFileHeader(string fileName, Guid broadcastId, 
            IFileMessageConfig fileMessageConfig,
            long firstPayloadIndex, 
            bool? isLast = null);
        IChunkHeader GetChunkHeader(string fileName, Guid broadcastId, BigInteger chunkIndex, bool? isLast = null);
        [Obsolete("This method has been derpecated.")]
        IEnumerable<IPayloadMessage> GetPayloadsByChunkIndex(string fileName, Guid broadcastId, BigInteger chunkIndex);
        /// <summary>
        /// Returns an observable that represents all of the payloads from a given file
        /// </summary>
        /// <param name="fileName">the fire to read</param>
        /// <param name="broadcastId">the id for the current broadcast</param>
        /// <param name="startingPayloadIndex">Optional - If this is not the first payload in the broadcast: specify at what payload index this file should start</param>
        /// <param name="startingBytes">Optional - If this is not the first payload in the broadcast: specify any bytes which should be prepended to the first payload of this series</param>
        /// <returns></returns>
        IObservable<IPayloadMessage> GetPayloads(string fileName, 
            Guid broadcastId, 
            long startingPayloadIndex = 0,
            byte[] startingBytes = null);
    }
}