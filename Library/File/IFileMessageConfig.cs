using System;

namespace Library.File
{
    public interface IFileMessageConfig
    {
        TimeSpan HeaderRebroadcastInterval { get; }
        long MaxPayloadSizeInBytes { get; }
    }
}