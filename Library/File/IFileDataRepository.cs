using System.Numerics;

namespace Library.File
{
    public interface IFileDataRepository
    {
        BigInteger GetByteCount(string fileName);
        byte[] GetPage(string fileName, uint pageSize, BigInteger pageIndex);
    }
}