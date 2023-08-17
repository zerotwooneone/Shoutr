using System;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;
using Shoutr.Contracts.Io;

namespace Shoutr.Contracts
{
    public interface IListener
    {
        /// <summary>
        /// Listens for broadcasts
        /// </summary>
        /// <param name="byteReceiver">The method of receiving bytes</param>
        /// <param name="streamFactory">The method of writing to a stream</param>
        /// <param name="destinationPath">The base path to which the stream is directed</param>
        /// <param name="cancellationToken">Allows the caller to cease litening</param>
        /// <returns>A task that represents the process of listening. This will complete after cancellation has been requested
        /// and all the in-progress work has completed.</returns>
        Task Listen(IByteReceiver byteReceiver,
            IStreamFactory streamFactory,
            string destinationPath = "",
            CancellationToken cancellationToken = default);

        event EventHandler<IBroadcastResult> BroadcastEnded;
    }
}