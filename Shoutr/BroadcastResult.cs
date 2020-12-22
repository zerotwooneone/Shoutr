using System;
using Shoutr.Contracts;

namespace Shoutr
{
    public record BroadcastResult : IBroadcastResult
    {
        public Guid BroadcastId { get; init; }
        public string FileName { get; init; }

        public BroadcastResult(Guid broadcastId, string fileName)
        {
            BroadcastId = broadcastId;
            FileName = fileName;
        }
    }
}