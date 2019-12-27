using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Library.Interface.Udp
{
    public interface IUdpListener : IDisposable
    {
        Task<UdpReceiveResult> ReceiveAsync();
    }
}