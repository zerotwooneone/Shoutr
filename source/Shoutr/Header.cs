using System;

namespace Shoutr
{
    internal record Header
    {
        internal Guid BroadcastId { get; init; }
        internal string FileName { get; init; }
        internal long PayloadMaxBytes { get; init; }
        internal long PayloadCount { get; init; }
    }
}