using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace Library.Broadcast
{
    class LanRepository : ILanRepository
    {
        private Queue<Task> q;
        private const int port = 3036;

        public void AddToQueue(Task broadcast)
        {
            q.Enqueue(broadcast);
        }

        public Task PopQueue()
        {
            throw new NotImplementedException();
        }

        public bool QueueIsEmpty { get; }

        public Task Broadcast(byte[] data)
        {
            UdpClient udp = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            udp.EnableBroadcast = true;
            return udp.SendAsync(data, data.Length, ip); //"Sends a UDP datagram asynchronously to a remote host."
        }

        public IEnumerable<byte[]> GetQueue()
        {
            throw new NotImplementedException("This method is obsolete.");
        }

        public EventHandler<UdpReceiveResult> OnReceived()
        {
            throw new NotImplementedException(); 
        }
    }
}
