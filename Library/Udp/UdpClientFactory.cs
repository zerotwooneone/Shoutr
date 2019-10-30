using System;
using System.Net;
using System.Net.Sockets;

namespace Library.Udp
{
    public class UdpClientFactory
    {
        public virtual IUdpListener CreateListener(IPEndPoint ipEndPoint)
        {
            return new UdpClientWrapper(new UdpClient(ipEndPoint));
        }

        internal IUdpBroadcaster CreateBroadcaster()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            return new UdpClientWrapper(udpClient);
        }
    }
}