using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;

namespace Shoutr.ByteTransport
{
    public class UdpBroadcastReceiver : IByteReceiver
    {
        public int Port { get; }

        protected UdpBroadcastReceiver(int port)
        {
            Port = port;
        }

        public static UdpBroadcastReceiver Factory(int port)
        {
            if(port < Defaults.MinPort || port > Defaults.MaxPort)
            {
                throw new ArgumentException(nameof(port));
            }

            return new UdpBroadcastReceiver(port);
        }
        public event EventHandler<IBytesReceived> BytesReceived;
        public async Task Listen(CancellationToken cancellationToken = default)
        {
            using var receiver = new UdpClient(Port);
            //receiver.EnableBroadcast = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                var receiveAsync = receiver.ReceiveAsync();
                //todo: this does not stop when the cancellation token signals cancellation
                var received = await receiveAsync.ConfigureAwait(false);
                Console.WriteLine($"Buff bytes {received.Buffer.Length}");
                OnBytesReceived(new BytesReceived(received.Buffer));
            }
        }

        protected virtual void OnBytesReceived(IBytesReceived e)
        {
            BytesReceived?.Invoke(this, e);
        }
    }
}