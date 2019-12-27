using System;
using System.Numerics;
using Library.Interface.ByteTransfer;

namespace Library.ByteTransfer
{
    public class ByteService: IByteService
    {
        public byte[] GetBytes(BigInteger bigInteger)
        {
            return GetBytesInternal(bigInteger);
        }

        public byte[] GetBytes(Guid guid)
        {
            return guid.ToByteArray();
        }

        public byte[] GetBytes(BigInteger? bigInteger)
        {
            return GetBytesInternal(bigInteger);
        }

        private byte[] GetBytesInternal(BigInteger? bigInteger)
        {
            return bigInteger?.ToByteArray();
        }

        public BigInteger GetBigInteger(byte[] bytes)
        {
            return new BigInteger(bytes);
        }

        public Guid GetGuid(byte[] bytes)
        {
            return new Guid(bytes);
        }

        public BigInteger? GetNullableBigInteger(byte[] bytes)
        {
            return bytes == null ? (BigInteger?) null : new BigInteger(bytes);
        }

        public long GetLong(byte[] bytes)
        {
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}
