using Shoutr.Contracts.ByteTransport;

namespace Shoutr.ByteTransport
{
    internal class BytesReceived : IBytesReceived
    {
        public byte[] Bytes { get; }

        public BytesReceived(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}