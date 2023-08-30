using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shoutr.Contracts.Io
{
    public interface IWriter : IDisposable
    {
        Task Write(long startIndex, byte[] bytes, CancellationToken cancellationToken);
    }
}