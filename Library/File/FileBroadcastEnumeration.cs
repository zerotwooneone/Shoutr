using System.Collections;
using System.Collections.Generic;
using Library.Message;

namespace Library.File
{
    public class FileBroadcastEnumeration : IEnumerable<byte[]>
    {
        private readonly IFileMessageService _fileMessageService;
        private readonly IBroadcastMessageConversionService _broadcastMessageConversionService;
        private readonly string _fileName;

        public FileBroadcastEnumeration(IFileMessageService fileMessageService,
            IBroadcastMessageConversionService broadcastMessageConversionService,
            string fileName)
        {
            _fileMessageService = fileMessageService;
            _broadcastMessageConversionService = broadcastMessageConversionService;
            _fileName = fileName;
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}