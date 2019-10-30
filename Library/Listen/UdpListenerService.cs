using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using Library.Message;
using Library.Udp;

namespace Library.Listen
{
    public class UdpListenerService : IListener
    {
        private readonly UdpClientFactory _udpClientFactory;
        private readonly IBroadcastMessageConversionService _broadcastMessageConversionService;
        private IObservable<UdpReceiveResult> _udpReceiveObservable;

        public UdpListenerService(IPAddress ipAddress, 
            int port, 
            UdpClientFactory udpClientFactory,
            IBroadcastMessageConversionService broadcastMessageConversionService)
        {
            _udpClientFactory = udpClientFactory;
            _broadcastMessageConversionService = broadcastMessageConversionService;
            IpEndPoint = new IPEndPoint(ipAddress, port);
            _udpReceiveObservable = Observable
                .Using(()=> _udpClientFactory.CreateListener(IpEndPoint),
                    udpClient=> Observable
                        .FromAsync(udpClient.ReceiveAsync)
                        .Repeat());
            MessagesObservable = _udpReceiveObservable
                .Select(ConvertToMessage);
        }

        public IReceivedMessage ConvertToMessage(UdpReceiveResult udpReceiveResult)
        {
            return new UdpReceivedMessage(_broadcastMessageConversionService.Convert(udpReceiveResult.Buffer), udpReceiveResult);
        }

        public IPEndPoint IpEndPoint { get; }
        public IObservable<IReceivedMessage> MessagesObservable { get; }
    }
}