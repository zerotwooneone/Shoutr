using System.Threading;
using System.Threading.Tasks;
using Shoutr.ByteTransport;
using Shoutr.Contracts;
using Shoutr.Io;

namespace Shoutr
{
    public static class BroadcasterExtensions
    {
        public static Task BroadcastFileUdp(this IBroadcaster broadcaster,
            string fileName,
            int port = Defaults.Port,
            int mtu = Defaults.Mtu,
            float headerRebroadcastSeconds = 1,
            string subnet = "192.168.1.255",
            CancellationToken cancellationToken = default)
        {
            var udpBroadcastSender = UdpBroadcastSender.Factory(subnet, mtu, port);
            var streamFactory = new StreamFactory();
            return broadcaster.BroadcastFile(fileName, 
                udpBroadcastSender,
                streamFactory,
                headerRebroadcastSeconds, cancellationToken);
        }
    }
}