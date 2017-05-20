using System.Collections.Generic;

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