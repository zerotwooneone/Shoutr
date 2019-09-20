using System.Net.Sockets;
using System.Threading.Tasks;

namespace Library.Listen
{
    public class UdpClientWrapper : IUdpListener
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
    }
}