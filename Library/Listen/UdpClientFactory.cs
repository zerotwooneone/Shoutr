using System.Net;
using System.Net.Sockets;

namespace Library.Listen
{
    public class UdpClientFactory
    {
        public virtual IUdpListener Create(IPEndPoint ipEndPoint)
        {
            return new UdpClientWrapper(new UdpClient(ipEndPoint));
        }
    }
}