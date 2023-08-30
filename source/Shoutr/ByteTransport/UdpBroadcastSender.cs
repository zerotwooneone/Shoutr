using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;

namespace Shoutr.ByteTransport
{
    public class UdpBroadcastSender : IByteSender, IDisposable
    {
        public IPEndPoint BroadcastEndpoint { get; }
        private readonly UdpClient _udpClient;
        public int MaximumTransmittableBytes { get; }

        protected UdpBroadcastSender(UdpClient udpClient,
            IPEndPoint broadcastEndpoint, int maximumTransmittableBytes)
        {
            BroadcastEndpoint = broadcastEndpoint;
            MaximumTransmittableBytes = maximumTransmittableBytes;
            _udpClient = udpClient;
        }

        public static UdpBroadcastSender Factory(string subnetIp = "192.168.1.255",
            int mtu = Defaults.Mtu,
            int port = Defaults.Port)
        {
            const int minMtu = 1;
            if (mtu < minMtu)
            {
                throw new ArgumentOutOfRangeException(nameof(mtu));
            }
            if(port < Defaults.MinPort || port > Defaults.MaxPort)
            {
                throw new ArgumentException(nameof(port));
            }
            if(!IPAddress.TryParse(subnetIp, out var subnetIpAddress))
            {
                throw new ArgumentException(nameof(subnetIp));
            }
            UdpClient sender = new UdpClient(port);
            sender.EnableBroadcast = true; //may not be needed
            IPEndPoint destination = new IPEndPoint(subnetIpAddress, port);
            return new UdpBroadcastSender(sender, destination, mtu);
        }

        public virtual async Task Send(byte[] bytes)
        {
            await _udpClient.SendAsync(bytes, bytes.Length, BroadcastEndpoint);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _udpClient?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}