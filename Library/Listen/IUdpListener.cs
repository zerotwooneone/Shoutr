using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Library.Listen
{
    public interface IUdpListener: IDisposable
    {
        Task<UdpReceiveResult> ReceiveAsync();
    }
}