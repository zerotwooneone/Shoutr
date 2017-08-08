using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Library.Message;

namespace Library.File
{
    /// <summary>
    /// Represents all the raw data that makes up a broadcast, based on one file. 
    /// This requests messages from the message service, in protocol order, 
    /// and passes those messages to the message conversion service so they become byte arrays.
    /// </summary>
    public class FileBroadcastEnumeration : IEnumerable<byte[]>
    {
        private readonly IFileMessageService _fileMessageService;
        private readonly IBroadcastMessageConversionService _broadcastMessageConversionService;
        private readonly string _fileName;
        private readonly Guid _broadcastId;

        public FileBroadcastEnumeration(IFileMessageService fileMessageService,
            IBroadcastMessageConversionService broadcastMessageConversionService,
            string fileName,
            Guid broadcastId)
        {
            _fileMessageService = fileMessageService;
            _broadcastMessageConversionService = broadcastMessageConversionService;
            _fileName = fileName;
            _broadcastId = broadcastId;
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