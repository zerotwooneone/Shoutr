using System.Collections.Generic;
using System.Numerics;

namespace Library.File
{
    public class FileChunk
    {
        public BigInteger ChunkIndex { get; set; }
        public IEnumerable<FilePayload> Payloads { get; set; }
    }
}