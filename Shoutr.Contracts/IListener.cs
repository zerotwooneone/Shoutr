using System;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;

namespace Shoutr.Contracts
{
    public interface IListener
    {
        Task Listen(IByteReceiver byteReceiver,
            CancellationToken cancellationToken = default);

        event EventHandler<IBroadcastResult> BroadcastEnded;
    }
}