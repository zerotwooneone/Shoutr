using System.IO;
using Shoutr.Contracts.Io;

namespace Shoutr.Io
{
    public class StreamFactory : IStreamFactory
    {
        public IReader CreateReader(string fileName)
        {
            var file = new FileInfo(fileName);
            return ReadableStream.Factory(file.OpenRead());
        }

        public IWriter CreateWriter(string fileName)
        {
            var file = new FileInfo(fileName);
            return WriteableStream.Factory(file.OpenWrite());
        }
    }
}