using System.Numerics;

namespace Library.File
{
    public interface IFileDataRepository
    {
        BigInteger GetByteCount(string fileName);
        byte[] GetPage(string fileName, ushort pageSize, BigInteger pageIndex);
    }
}