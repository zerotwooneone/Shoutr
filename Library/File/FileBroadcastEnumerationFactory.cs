using System.Collections.Generic;
using Library.Message;

namespace Library.File
{
    public class FileBroadcastEnumerationFactory
    {
        private readonly IFileMessageEnumerationFactory _fileMessageEnumerationFactory;
        private readonly IBroadcastMessageConversionService _broadcastMessageConversionService;

        public FileBroadcastEnumerationFactory(IFileMessageEnumerationFactory fileMessageEnumerationFactory,
            IBroadcastMessageConversionService broadcastMessageConversionService)
        {
            _fileMessageEnumerationFactory = fileMessageEnumerationFactory;
            _broadcastMessageConversionService = broadcastMessageConversionService;
        }
        public IEnumerable<byte[]> GetEnumeration(string fileName)
        {
            throw new System.NotImplementedException();
        }
    }
}