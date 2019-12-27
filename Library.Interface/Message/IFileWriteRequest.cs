using System.Numerics;

namespace Library.Interface.Message
{
    public interface IFileWriteRequest
    {
        BigInteger StartIndex { get; }
        byte[] Payload { get; }
        string FileName { get; }
    }
}