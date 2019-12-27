using System.Numerics;

namespace Library.Interface.File
{
    public class FilePayload
    {
        public BigInteger PayloadIndex { get; set; }
        public byte[] Bytes { get; set; }
    }
}