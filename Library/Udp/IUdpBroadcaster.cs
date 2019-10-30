using System;
using System.Net;
using System.Threading.Tasks;

namespace Library.Udp
{
    public interface IUdpBroadcaster : IDisposable
    {
        Task<int> SendAsync(byte[] data, IPEndPoint ipEndpoint);
    }

    public static class UdpBradcasterExtensions
    {
        public static async Task<int> BroadcastAsync(this IUdpBroadcaster udpBroadcaster, byte[] data, int port)
        {
            return await udpBroadcaster.SendAsync(data, new IPEndPoint(IPAddress.Any, port));
        }
    }
}