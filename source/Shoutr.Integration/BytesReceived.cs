using Shoutr.Contracts.ByteTransport;

namespace Shoutr.Integration
{
    public class BytesReceived : IBytesReceived
    {
        public byte[] Bytes { get; }

        public BytesReceived(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}