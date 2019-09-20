using System.Net;
using Library.Message;

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