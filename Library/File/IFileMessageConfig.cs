using System;

namespace Library.File
{
    public interface IFileMessageConfig
    {
        /// <summary>
        /// The time between header re-broadcasts
        /// </summary>
        TimeSpan HeaderRebroadcastInterval { get; }
        /// <summary>
        /// The maximum size a payload can be
        /// </summary>
        long MaxPayloadSizeInBytes { get; }
    }
}