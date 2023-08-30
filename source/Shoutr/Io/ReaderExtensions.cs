using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Shoutr.Contracts.Io;

namespace Shoutr.Io
{
    public static class ReaderExtensions
    {
        public static IObservable<Page> PageObservable(this IReader reader, int pageSize, CancellationToken token)
        {
            return Observable.While(
                // while there is data to be read
                () => reader.Position < reader.Length,
                // iteratively invoke the observable factory, which will
                // "recreate" it such that it will start from the current
                // stream position - hence "0" for offset
                Observable.FromAsync(async () =>
                    {
                        var buffer = new byte[pageSize];
                        var readBytes = await reader.Read(buffer, pageSize, token).ConfigureAwait(false);
                        return new
                        {
                            readBytes,
                            startIndex = reader.Position - readBytes,
                            buffer
                        };
                    })
                    .Select(readResult =>
                    {
                        return new Page
                        {
                            Bytes = readResult.buffer.Take((int)readResult.readBytes).ToArray(),
                            PageIndex = readResult.startIndex
                        };
                    }));
        }
    }
}