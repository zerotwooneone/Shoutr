using System;

namespace Shoutr.Contracts
{
    public interface IBroadcastResult
    {
        Guid BroadcastId { get; }
        string FileName { get; }
    }
}