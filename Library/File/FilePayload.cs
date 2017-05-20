using System.Numerics;

namespace Library.File
{
    public class FilePayload
    {
        public BigInteger PayloadId { get; set; }
        public byte[] Bytes { get; set; }
    }
}