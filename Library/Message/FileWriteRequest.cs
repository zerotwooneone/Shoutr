using System;
using System.Numerics;

namespace Library.Message
{
    public class FileWriteRequest : IFileWriteRequest
    {
        public FileWriteRequest(BigInteger startIndex, 
            byte[] payload, 
            string fileName)
        {
            StartIndex = startIndex;
            Payload = payload;
            FileName = fileName;
        }
        
        public BigInteger StartIndex { get; }
        public byte[] Payload { get; }
        public string FileName { get; }
    }
}