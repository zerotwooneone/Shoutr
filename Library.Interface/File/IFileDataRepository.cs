using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Interface.File
{
    /// <summary>
    /// Represents access to all pages of all files
    /// </summary>
    public interface IFileDataRepository
    {
        long GetByteCount(string fileName);
        
        Task<FileWriteResult> SetPage(string fileName, BigInteger startIndex, byte[] payload, CancellationToken cancellationToken = default);
        Task<FileReadResult> GetPage(string fileName, int pageSize, long pageIndex, CancellationToken cancellationToken = default);
    }
}