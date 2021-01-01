using System.Threading;
using System.Threading.Tasks;
using Shoutr.ByteTransport;
using Shoutr.Contracts;

namespace Shoutr
{
    public static class ListenerExtensions
    {
        public static Task ListenUdpBroadcast(this IListener listener, 
            int port = Defaults.Port,
            CancellationToken cancellationToken = default)
        {
            return listener.Listen(UdpBroadcastReceiver.Factory(port),
                cancellationToken);
        }
    }
}