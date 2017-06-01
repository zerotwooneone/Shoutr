using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Library.Broadcast
{
    class LanRepository : ILanRepository
    {
        private Queue<byte[]> q;
        private const int port = 3036;

        public void AddToQueue(byte[] data)
        {
            //throw new NotImplementedException();
            q.Enqueue(data);
        }

        public byte[] PopQueue()
        {
            throw new NotImplementedException();
        }

        public bool QueueIsEmpty { get; }

        public Task Broadcast(byte[] data)
        {
            //throw new NotImplementedException();
            UdpClient udp = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            udp.EnableBroadcast = true;
            return udp.SendAsync(data, data.Length, ip); //"Sends a UDP datagram asynchronously to a remote host."
        }

        public IEnumerable<byte[]> GetQueue()
        {
            //throw new NotImplementedException();
            return q;
        }

        public EventHandler<UdpReceiveResult> OnReceived()
        {
            throw new NotImplementedException();
            //write to something?
        }
    }
}
