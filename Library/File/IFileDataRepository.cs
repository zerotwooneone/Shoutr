using System.Numerics;

namespace Library.File
{
    /// <summary>
    /// Represents access to all pages of all files
    /// </summary>
    public interface IFileDataRepository
    {
        long GetByteCount(string fileName);
        byte[] GetPage(string fileName, uint pageSize, BigInteger pageIndex);
    }
}