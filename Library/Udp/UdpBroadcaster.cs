using Library.Message;
using System.Threading.Tasks;
using Library.Interface.Broadcast;
using Library.Interface.Message;
using Library.Interface.Udp;

namespace Library.Udp
{
    public class UdpBroadcaster : IBroadcaster
    {
        private readonly IUdpBroadcaster _udpBroadcaster;
        private readonly BroadcastMessageConversionService _broadcastMessageConversionService;
        private readonly IUdpBroadcastThrottleConfig _udpBroadcastThrottleConfig;

        public int Port { get; }

        public UdpBroadcaster(IUdpBroadcaster udpBroadcaster,
            BroadcastMessageConversionService broadcastMessageConversionService,
            int port,
            IUdpBroadcastThrottleConfig udpBroadcastThrottleConfig)
        {
            _udpBroadcaster = udpBroadcaster;
            _broadcastMessageConversionService = broadcastMessageConversionService;
            Port = port;
            _udpBroadcastThrottleConfig = udpBroadcastThrottleConfig;
        }

        public async Task Broadcast(IBroadcastHeader broadcastHeader)
        {
            var data = _broadcastMessageConversionService.Convert(broadcastHeader);
            var bytesSent = await _udpBroadcaster.BroadcastAsync(data, Port);
        }

        public async Task Broadcast(IFileHeader fileHeader)
        {
            var data = _broadcastMessageConversionService.Convert(fileHeader);
            var bytesSent = await _udpBroadcaster.BroadcastAsync(data, Port);
        }

        public async Task Broadcast(IChunkHeader chunkHeader)
        {
            var data = _broadcastMessageConversionService.Convert(chunkHeader);
            var bytesSent = await _udpBroadcaster.BroadcastAsync(data, Port);
        }

        public async Task Broadcast(IPayloadMessage payloadMessage)
        {
            var data = _broadcastMessageConversionService.Convert(payloadMessage);
            var bytesSent = await _udpBroadcaster.BroadcastAsync(data, Port);
        }
    }
}