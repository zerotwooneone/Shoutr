using System;
using System.Numerics;

namespace Library.Message
{
    public class BroadcastMessage
    {
        public Guid BroadcastId { get; set; }
        public BigInteger? ChunkId { get; set; }
        public BigInteger? PayloadId { get; set; }
        public byte[] Payload { get; set; }
        public bool? IsLast { get; set; }

        public ushort? ChunkSize { get; set; }
        public string FileName { get; set; }
        public BigInteger? ChunkCount { get; set; }
    }
}