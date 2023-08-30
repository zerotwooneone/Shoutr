using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.Io;

namespace Shoutr.Io
{
    public class ReadableStream : IReader
    {
        private readonly FileStream _stream;

        protected ReadableStream(FileStream stream)
        {
            _stream = stream;
        }

        public long Position => _stream.Position;
        public long Length => _stream.Length;
        public async Task<long> Read(byte[] buffer, int readLength, CancellationToken cancellationToken)
        {
            return await _stream.ReadAsync(buffer, 0, readLength, cancellationToken).ConfigureAwait(false);
        }

        public static ReadableStream Factory([NotNull] FileStream fileStream)
        {
            if (!fileStream.CanRead || !fileStream.CanSeek)
            {
                throw new ArgumentException(nameof(fileStream));
            }

            return new ReadableStream(fileStream);
        }
    }
}