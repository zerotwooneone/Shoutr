using System.Numerics;

namespace Library.File
{
    public class FilePayload
    {
        public BigInteger PayloadIndex { get; set; }
        public byte[] Bytes { get; set; }
    }
}