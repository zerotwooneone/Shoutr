using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Library.Udp
{
    public interface IUdpListener : IDisposable
    {
        Task<UdpReceiveResult> ReceiveAsync();
    }
}