using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Library.Broadcast
{
    public interface ILanRepository
    {
        Task Broadcast(byte[] data);
        EventHandler<UdpReceiveResult> OnReceived();
    }
}
