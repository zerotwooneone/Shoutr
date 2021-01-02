using System;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;
using Shoutr.Contracts.Io;

namespace Shoutr.Contracts
{
    public interface IListener
    {
        Task Listen(IByteReceiver byteReceiver,
            IStreamFactory streamFactory,
            CancellationToken cancellationToken = default);

        event EventHandler<IBroadcastResult> BroadcastEnded;
    }
}