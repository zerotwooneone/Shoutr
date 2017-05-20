using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Library.Broadcast
{
    public interface ILanRepository
    {
        Task Broadcast(byte[] data);
        EventHandler<UdpReceiveResult> OnReceived();
        IEnumerable<byte[]> GetQueue();
        void AddToQueue(byte[] data);
    }
}
