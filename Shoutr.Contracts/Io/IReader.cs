using System.Threading;
using System.Threading.Tasks;

namespace Shoutr.Contracts.Io
{
    public interface IReader {
        long Position { get; }
        long Length { get; }
        Task<long> Read(byte[] buffer,int readLength, CancellationToken cancellationToken);
    }
}