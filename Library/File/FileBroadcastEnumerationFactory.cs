using System.Collections.Generic;
using Library.Message;

namespace Library.File
{
    public class FileBroadcastEnumerationFactory
    {
        private readonly IFileMessageService _fileMessageService;
        private readonly IBroadcastMessageConversionService _broadcastMessageConversionService;

        public FileBroadcastEnumerationFactory(IFileMessageService fileMessageService,
            IBroadcastMessageConversionService broadcastMessageConversionService)
        {
            _fileMessageService = fileMessageService;
            _broadcastMessageConversionService = broadcastMessageConversionService;
        }
        public IEnumerable<byte[]> GetEnumeration(string fileName)
        {
            throw new System.NotImplementedException();
        }
    }
}