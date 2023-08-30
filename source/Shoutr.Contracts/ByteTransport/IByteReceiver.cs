using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shoutr.Contracts.ByteTransport
{
    /// <summary>
    /// Represents a listener which will provide bytes from some source
    /// </summary>
    public interface IByteReceiver
    {
        /// <summary>
        /// Raised when there are new bytes from the source
        /// </summary>
        event EventHandler<IBytesReceived> BytesReceived;
        /// <summary>
        /// Begins Listening. This will block the current thread while listening. Subsequent calls to listen while already listening may cause unexpected behavior.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A Task that represents the work of begining to listen</returns>
        Task Listen(CancellationToken cancellationToken = default);
    }
}