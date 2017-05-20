using System;
using System.Numerics;

namespace Library.ByteTransfer
{
    public class ByteService
    {
        public byte[] GetBytes(BigInteger bigInteger)
        {
            return bigInteger.ToByteArray();
        }

        public byte[] GetBytes(Guid guid)
        {
            return guid.ToByteArray();
        }
    }
}
