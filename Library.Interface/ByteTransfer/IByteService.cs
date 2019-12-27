using System;
using System.Numerics;

namespace Library.Interface.ByteTransfer
{
    /// <summary>
    /// Converts between bytes and common data types. This is primarily focused on addressing the lack of built-in serialization for some data type.
    /// </summary>
    public interface IByteService
    {
        byte[] GetBytes(BigInteger bigInteger);
        byte[] GetBytes(Guid guid);
        byte[] GetBytes(BigInteger? payloadIndex);
        BigInteger GetBigInteger(byte[] bytes);
        Guid GetGuid(byte[] bytes);
        BigInteger? GetNullableBigInteger(byte[] bytes);
        long GetLong(byte[] bytes);
    }
}