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
        private Queue<byte[]> q = new Queue<byte[]>();
        private const int port = 3036;

        public void AddToQueue(byte[] broadcast)
        {
            q.Enqueue(broadcast);
        }

        public byte[] PopQueue()
        {
            return q.Dequeue();
        }

        public bool QueueIsEmpty => !q.Any();
        public Task DequeueTask { get; set; }

        public Task Broadcast(byte[] data)
        {
            UdpClient udp = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            udp.EnableBroadcast = true;
            return new Task(async () => await udp.SendAsync(data, data.Length, ip)); //"Sends a UDP datagram asynchronously to a remote host."
        }

        public EventHandler<UdpReceiveResult> OnReceived()
        {
            throw new NotImplementedException(); 
        }
    }
}
