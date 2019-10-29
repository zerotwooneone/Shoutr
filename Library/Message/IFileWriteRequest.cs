using System;
using System.Numerics;

namespace Library.Message
{
    public interface IFileWriteRequest
    {
        BigInteger StartIndex { get; }
        byte[] Payload { get; }
        string FileName { get; }
    }
}