using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.Io;

namespace Shoutr.Io
{
    public class WriteableStream : IWriter
    {
        private readonly FileStream _fileStream;

        private WriteableStream(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        public static WriteableStream Factory([NotNull] FileStream fileStream)
        {
            if (fileStream is null || !fileStream.CanWrite || !fileStream.CanSeek)
            {
                throw new ArgumentException(nameof(fileStream));
            }
            return new WriteableStream(fileStream);
        }

        public async Task Write(long startIndex, byte[] bytes, CancellationToken cancellationToken)
        {
            _fileStream.Seek(startIndex, SeekOrigin.Begin);
            await _fileStream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileStream.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}