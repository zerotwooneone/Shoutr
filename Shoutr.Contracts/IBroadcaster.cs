using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;

namespace Shoutr.Contracts
{
    public interface IBroadcaster
    {
        /// <summary>
        /// Broadcasts one file
        /// </summary>
        /// <param name="fileName">the full path to the file</param>
        /// <param name="byteSender">the method of sending bytes to receivers</param>
        /// <param name="headerRebroadcastSeconds">seconds between rebroadcasting the headers</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task BroadcastFile(string fileName,
            IByteSender byteSender,
            float headerRebroadcastSeconds = 1,
            CancellationToken cancellationToken = default);
    }
}
