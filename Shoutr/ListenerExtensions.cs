using System.Threading;
using System.Threading.Tasks;
using Shoutr.ByteTransport;
using Shoutr.Contracts;
using Shoutr.Io;

namespace Shoutr
{
    public static class ListenerExtensions
    {
        public static Task ListenUdpBroadcast(this IListener listener, 
            int port = Defaults.Port,
            string destinationPath = "", 
            CancellationToken cancellationToken = default)
        {
            var byteReceiver = UdpBroadcastReceiver.Factory(port);
            var streamFactory= new StreamFactory();
            return listener.Listen(byteReceiver,
                streamFactory,
                destinationPath,
                cancellationToken);
        }
    }
}