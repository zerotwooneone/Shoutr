using System.Net.Sockets;
using Library.Interface.Message;

namespace Library.Message
{
    public class UdpReceivedMessage : Messages, IReceivedMessage
    {
        public UdpReceivedMessage(Messages messages,
            UdpReceiveResult udpReceiveResult)
        {
            BroadcastHeader = messages.BroadcastHeader;
            ChunkHeader = messages.ChunkHeader;
            FileHeader = messages.FileHeader;
            PayloadMessage = messages.PayloadMessage;
            SenderId = udpReceiveResult.RemoteEndPoint.ToString();
        }
        public string SenderId { get; }
    }
}