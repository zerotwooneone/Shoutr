using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Library.Udp
{
    public class UdpClientWrapper : IUdpListener, IUdpBroadcaster
    {
        private readonly UdpClient _udpClient;

        public UdpClientWrapper(UdpClient udpClient)
        {
            _udpClient = udpClient;
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }

        public Task<UdpReceiveResult> ReceiveAsync()
        {
            return _udpClient.ReceiveAsync();
        }

        public async Task<int> SendAsync(byte[] data, IPEndPoint ipEndpoint)
        {
            return await _udpClient.SendAsync(data, data.Length,ipEndpoint);
        }
    }
}