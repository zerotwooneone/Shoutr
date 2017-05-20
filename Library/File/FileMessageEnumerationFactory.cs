using System;
using System.Collections.Generic;
using System.IO;
using Library.Message;

namespace Library.File
{
    public class FileMessageEnumerationFactory: IFileMessageEnumerationFactory
    {
        private readonly IFileDataFactory _fileDataFactory;

        public FileMessageEnumerationFactory(IFileDataFactory fileDataFactory)
        {
            _fileDataFactory = fileDataFactory;
        }

        public IEnumerable<BroadcastMessage> Create(string fileName, Guid broadcastId)
        {
            throw new System.NotImplementedException();
        }
    }
}