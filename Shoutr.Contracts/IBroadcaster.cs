using System.Threading;
using System.Threading.Tasks;

namespace Shoutr.Contracts
{
    public interface IBroadcaster
    {
        /// <summary>
        /// Broadcasts one file
        /// </summary>
        /// <param name="fileName">the full path to the file</param>
        /// <param name="port">the UDP port on which to broadcast</param>
        /// <param name="mtu">the maximum transmission unit</param>
        /// <param name="headerRebroadcastSeconds">seconds between rebroadcasting the headers</param>
        /// <param name="subnet">the ipv4 address of the local subnet</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task BroadcastFile(string fileName,
            int port = 3036,
            int mtu = 1400,
            float headerRebroadcastSeconds = 1,
            string subnet = "192.168.1.255",
            CancellationToken cancellationToken = default);
    }
}
