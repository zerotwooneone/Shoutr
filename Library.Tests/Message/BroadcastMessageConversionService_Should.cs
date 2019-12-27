using System;
using Library.Interface.ByteTransfer;
using Library.Interface.Message;
using Library.Message;
using Moq;

namespace Library.Tests.Message
{
    public class BroadcastMessageConversionService_OneByteFileShould
    {
        private readonly BroadcastMessageConversionService _broadcastMessageConversionService;
        private IBroadcastHeader _broadcastHeader;
        private IFileHeader _fileHeader;
        private IPayloadMessage _payloadMessage;
        private readonly Mock<IByteService> _byteService;

        public BroadcastMessageConversionService_OneByteFileShould()
        {
            var broadcastId = Guid.NewGuid();
            bool? isLast = null;
            const int chunkSizeInBytes = 1;
            _broadcastHeader = new BroadcastHeader(broadcastId, isLast, chunkSizeInBytes);
            const string fileName = "fileName";
            const int chunkCount = 1;
            _fileHeader = new FileHeader(broadcastId, isLast, fileName, chunkCount);
            const int payloadIndex = 0;
            var payload = new byte[] { 1 };
            const int chunkIndex = 0;
            _payloadMessage = new PayloadMessage(broadcastId, payloadIndex, payload, chunkIndex);
            _byteService = new Mock<IByteService>();
            _broadcastMessageConversionService = new BroadcastMessageConversionService(_byteService.Object);
        }

        
    }
}