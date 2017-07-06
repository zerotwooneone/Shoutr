using System;
using System.Collections.Generic;
using System.Numerics;
using Library.Configuration;
using Library.Message;
using System.IO;
using Library.File;


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

    public IBroadcastHeader GetBroadcastHeader (string fileName, Guid broadcastId, bool? isLast = null)
    {
        long size = _fileDataRepository.GetByteCount(fileName);
        BroadcastHeader header = new BroadcastHeader(broadcastId, isLast, size);
        return header;
    }

    public IFileHeader GetFileHeader(string fileName, Guid broadcastId, bool? isLast = null)
    {
        BigInteger chunkCount = 1;
        FileHeader header = new FileHeader(broadcastId, isLast, fileName, chunkCount);
        return header;
    }

    public IChunkHeader GetChunkHeader(string fileName, Guid broadcastId, BigInteger chunkIndex, bool? isLast = null)
    {
        return new ChunkHeader(broadcastId, chunkIndex, isLast);
    }
        
    public IEnumerable<IPayloadMessage> GetPayloadsByChunkIndex(string fileName, Guid broadcastId, BigInteger chunkIndex)
    {
        throw new NotImplementedException();
    }
}
 