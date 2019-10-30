using Library.Message;
using Library.Udp;

namespace Library.Broadcast
{
    public class BroadcasterFactory
    {
        private readonly UdpClientFactory _udpClientFactory;
        private readonly BroadcastMessageConversionService _broadcastMessageConversionService;

        public BroadcasterFactory(UdpClientFactory udpClientFactory,
            BroadcastMessageConversionService broadcastMessageConversionService)
        {
            _udpClientFactory = udpClientFactory;
            _broadcastMessageConversionService = broadcastMessageConversionService;
        }
        public IBroadcaster CreateUdpBroadcaster(int port, IUdpBroadcastThrottleConfig udpBroadcastThrottleConfig)
        {
            var udpClient = _udpClientFactory.CreateBroadcaster();
            return new UdpBroadcaster(udpClient, 
                _broadcastMessageConversionService, 
                port,
                udpBroadcastThrottleConfig);
        }
    }
}