using System;
using System.Collections.Generic;
using System.Numerics;
using Library.Configuration;
using Library.Message;

namespace Library.File
{
    public class FileMessageService : IFileMessageService
    {
        private readonly IFileDataRepository _fileDataRepository;
        private readonly IConfigurationService _configurationService;

        public FileMessageService(IFileDataRepository fileDataRepository,
            IConfigurationService configurationService)
        {
            _fileDataRepository = fileDataRepository;
            _configurationService = configurationService;
        }

        public IBroadcastHeader GetBroadcastHeader(string fileName, Guid broadcastId,
                IFileMessageConfig fileMessageConfig, bool? isLast = null)
        {
            long fileSize = _fileDataRepository.GetByteCount(fileName);
            var maxPayloadSizeInBytes = Math.Min(fileSize, fileMessageConfig.MaxPayloadSizeInBytes);
            BroadcastHeader header = new BroadcastHeader(broadcastId, isLast, maxPayloadSizeInBytes);
            return header;
        }

        public IFileHeader GetFileHeader(string fileName,
            Guid broadcastId,
            IFileMessageConfig fileMessageConfig,
            long firstPayloadIndex,
            bool? isLast = null)
        {
            FileHeader header = new FileHeader(broadcastId, isLast, fileName, firstPayloadIndex);
            return header;
        }

        public IChunkHeader GetChunkHeader(string fileName, Guid broadcastId, BigInteger chunkIndex, bool? isLast = null)
        {
            return new ChunkHeader(broadcastId, chunkIndex, isLast);
        }

        public IEnumerable<IPayloadMessage> GetPayloadsByChunkIndex(string fileName, Guid broadcastId, BigInteger chunkIndex)
        {
            //BigInteger pageIndex = 0;
            long byteCount = _fileDataRepository.GetByteCount(fileName);
            uint pageSize = _configurationService.PageSize;
            int payloadSize = _configurationService.PayloadSizeInBytes;
            byte[] payload = new byte[payloadSize];
            BigInteger payloadIndex = 0;
            int i = 0;
            int copied = 0;
            long pages = byteCount % pageSize == 0 ? byteCount / pageSize : byteCount / pageSize + 1;
            for (BigInteger pageIndex = 0; pageIndex < pages; pageIndex++)
            {
                var page = _fileDataRepository.GetPage(fileName, pageSize, pageIndex);
                for (int j = 0; j < pageSize; i += copied, j += copied)
                {
                    copied = (int)(payloadSize - i < pageSize - j ? payloadSize - i : pageSize - j);
                    copied = copied < page.Length ? copied : page.Length;
                    Array.Copy(page, j, payload, i, copied);
                    if (i == payloadSize - 1)
                    {
                        IPayloadMessage pm = new PayloadMessage(broadcastId, payloadIndex, payload, chunkIndex);
                        payloadIndex++;
                        i = 0;
                        yield return pm;
                    }
                }
            }
        }

        public IObservable<IPayloadMessage> GetPayloads(string fileName, Guid broadcastId, long startingPayloadIndex = 0, byte[] startingBytes = null)
        {
            throw new NotImplementedException();
        }
    }
}