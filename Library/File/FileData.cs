using System.Collections.Generic;
using System.Numerics;

namespace Library.File
{
    public class FileData
    {
        public string FileName { get; set; }
        public BigInteger ChunkCount { get; set; }
        public IEnumerable<FileChunk> FileChunks { get; set; }
    }
}