using System;
using System.Collections.Generic;
using System.Numerics;
using Library.Configuration;
using Library.Message;

namespace Library.File
{
    public class FileMessageService: IFileMessageService
    {
        private readonly IFileDataRepository _fileDataRepository;
        private readonly IConfigurationService _configurationService;

        public FileMessageService(IFileDataRepository fileDataRepository,
            IConfigurationService configurationService)
        {
            _fileDataRepository = fileDataRepository;
            _configurationService = configurationService;
        }

        public IBroadcastHeader GetBroadcastHeader(string fileName, Guid broadcastId)
        {
            throw new NotImplementedException();
        }

        public IFileHeader GetFileHeader(string fileName, Guid broadcastId)
        {
            throw new NotImplementedException();
        }

        public IChunkHeader GetChunkHeader(string fileName, Guid broadcastId, BigInteger chunkIndex)
        {
            throw new NotImplementedException();
        }
        
        public IEnumerable<IPayloadMessage> GetPayloadsByChunkIndex(string fileName, Guid broadcastId, BigInteger chunkIndex)
        {
            throw new NotImplementedException();
        }
    }
}