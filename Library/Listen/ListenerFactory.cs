using System.Net;
using Library.Interface.Listen;
using Library.Interface.Message;
using Library.Udp;

namespace Library.Listen
{
    public class ListenerFactory : IListenerFactory
    {
        private readonly UdpClientFactory _udpClientFactory;
        private readonly IBroadcastMessageConversionService _broadcastMessageConversionService;

        public ListenerFactory(UdpClientFactory udpClientFactory,
            IBroadcastMessageConversionService broadcastMessageConversionService)
        {
            _udpClientFactory = udpClientFactory;
            _broadcastMessageConversionService = broadcastMessageConversionService;
        }
        public IListener CreateBroadcastListener(int port)
        {
            return new UdpListenerService(IPAddress.Any, 
                port, 
                _udpClientFactory, 
                _broadcastMessageConversionService);
        }
    }
}