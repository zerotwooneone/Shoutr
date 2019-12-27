using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using Library.Interface.Configuration;
using Library.Interface.File;
using Library.Interface.Message;

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

        public IObservable<IPayloadMessage> GetPayloads(string fileName, 
            Guid broadcastId,
            IFileMessageConfig fileMessageConfig, 
            long startingPayloadIndex = 0,
            byte[] startingBytes = null)
        {
            var bytesInFile = _fileDataRepository.GetByteCount(fileName);
            
            const int convertCountToIndex = 1;
            var lastByteIndexInFile = bytesInFile - convertCountToIndex;
            
            var totalPageCount = (int)Math.Ceiling( bytesInFile / (double)_configurationService.PageSize);

            var allPageIndexes = Enumerable
                .Range(0, totalPageCount);
            var filePageDictionary = allPageIndexes
                .ToDictionary(pageIndex=>pageIndex,
                pageIndex=>{
                    return Observable.FromAsync(() =>
                    {
                        return _fileDataRepository.GetPage(fileName, (int)_configurationService.PageSize, pageIndex);
                    });
                 });

            
            //need to handle errors

            var payloadCount = (int)Math.Ceiling(bytesInFile / (double)fileMessageConfig.MaxPayloadSizeInBytes);
            var payloadIndexObservable = Observable
                .Range((int)startingPayloadIndex, payloadCount);

            var payloadObservable = payloadIndexObservable
                .Select(payloadIndex=>{ 
                    var firstPayloadByteIndex = (payloadIndex - startingPayloadIndex) * fileMessageConfig.MaxPayloadSizeInBytes;
                    var lastPayloadByteIndex = Math.Min(lastByteIndexInFile, firstPayloadByteIndex + fileMessageConfig.MaxPayloadSizeInBytes);

                    int firstPayloadPageIndex = (int)(firstPayloadByteIndex / _configurationService.PageSize);
                    int lastPayloadPageIndex = (int)(lastPayloadByteIndex / _configurationService.PageSize);

                    const int convertIndexToCount = 1;
                    int pageCount = lastPayloadPageIndex - firstPayloadPageIndex + convertIndexToCount;
                    
                    var pageReadObservable = Observable.Range(firstPayloadPageIndex, pageCount)
                        .Select(pageIndex=>{ 
                            return filePageDictionary[pageIndex]
                                .Select(fileReadResult=>{ 
                                    var filePage = new FilePage(fileReadResult, pageIndex);
                                    return filePage;
                                });
                        })
                        .Merge(maxConcurrent: 1);

                    var currentPayloadObservable = pageReadObservable
                        .Select(filePage =>
                        {
                            var bytes = (filePage.PageIndex == firstPayloadPageIndex
                                ? (startingBytes == null 
                                    ? filePage.FileReadResult.Bytes
                                    : startingBytes.AsEnumerable().Concat(filePage.FileReadResult.Bytes))
                                : filePage.FileReadResult.Bytes)
                                .ToArray();
                            var payloadSize = Math.Min((int)fileMessageConfig.MaxPayloadSizeInBytes, bytes.Length);
                            var payload = bytes.Take(payloadSize).ToArray();
                            const int unusedChunkIndex = 0;
                            return new PayloadMessage(broadcastId, payloadIndex, payload, unusedChunkIndex);
                        });
                    return currentPayloadObservable;
                 })
                .Merge();
                
            return payloadObservable;
        }

        internal class FilePage
        {
            public FileReadResult FileReadResult { get; }
            public long PageIndex { get; }
            public FilePage(FileReadResult fileReadResult, long pageIndex)
            {
                FileReadResult = fileReadResult;
                PageIndex = pageIndex;
            }            
        }
    }
}