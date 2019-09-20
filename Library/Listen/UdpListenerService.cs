using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using Library.Message;

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
                .Using(()=> _udpClientFactory.Create(IpEndPoint),
                    udpClient=> Observable
                        .FromAsync(udpClient.ReceiveAsync)
                        .Repeat());
            MessagesObservable = _udpReceiveObservable.Select(ConvertToMessage);
        }

        public Messages ConvertToMessage(UdpReceiveResult arg)
        {
            return _broadcastMessageConversionService.Convert(arg.Buffer);
        }

        public IPEndPoint IpEndPoint { get; }
        public IObservable<Messages> MessagesObservable { get; }
    }
}