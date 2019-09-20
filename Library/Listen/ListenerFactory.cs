using System.Net;
using Library.Message;

namespace Library.Listen
{
    public class ListenerFactory : IListenerFactory
    {
        private readonly UdpClientFactory _udpClientFactory;
        private readonly BroadcastMessageConversionService _broadcastMessageConversionService;

        public ListenerFactory(UdpClientFactory udpClientFactory,
            BroadcastMessageConversionService broadcastMessageConversionService)
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