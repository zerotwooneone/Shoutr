using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace Library.Broadcast
{
    public class LanRepository : ILanRepository
    {
        private Queue<Task> q = new Queue<Task>();
        private const int port = 3036;

        public void AddToQueue(Task broadcast)
        {
            q.Enqueue(broadcast);
        }

        public Task PopQueue()
        {
            return q.Dequeue();
        }

        public bool QueueIsEmpty => !q.Any();

        public Task Broadcast(byte[] data)
        {
            UdpClient udp = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            udp.EnableBroadcast = true;
            return udp.SendAsync(data, data.Length, ip); //"Sends a UDP datagram asynchronously to a remote host."
        }

        public EventHandler<UdpReceiveResult> OnReceived()
        {
            throw new NotImplementedException(); 
        }
    }
}
