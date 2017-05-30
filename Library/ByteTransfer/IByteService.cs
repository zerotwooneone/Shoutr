using System;
using System.Numerics;

namespace Library.ByteTransfer
{
    /// <summary>
    /// Converts between bytes and common data types. This is primarily focused on addressing the lack of built-in serialization for some data type.
    /// </summary>
    public interface IByteService
    {
        byte[] GetBytes(BigInteger bigInteger);
        byte[] GetBytes(Guid guid);
        byte[] GetBytes(BigInteger? payloadIndex);
    }
}