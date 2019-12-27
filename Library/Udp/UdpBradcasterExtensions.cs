using System.Net;
using System.Threading.Tasks;
using Library.Interface.Udp;

namespace Library.Udp
{
    public static class UdpBradcasterExtensions
    {
        public static async Task<int> BroadcastAsync(this IUdpBroadcaster udpBroadcaster, byte[] data, int port)
        {
            return await udpBroadcaster.SendAsync(data, new IPEndPoint(IPAddress.Any, port));
        }
    }
}