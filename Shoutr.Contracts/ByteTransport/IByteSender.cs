using System.Threading.Tasks;

namespace Shoutr.Contracts.ByteTransport
{
    public interface IByteSender
    {
        /// <summary>
        /// The maximum number of bytes when can be sent to the send method
        /// </summary>
        int MaximumTransmittableBytes { get; }
        Task Send(byte[] bytes);
    }
}