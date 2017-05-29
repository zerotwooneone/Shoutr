using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Library.Message;
using Xunit;

namespace Library.Tests.Message
{
    public class BroadcastMessageConversionService_OneByteFileShould
    {
        private readonly BroadcastMessageConversionService _broadcastMessageConversionService;
        private IBroadcastHeader _broadcastHeader;
        private IFileHeader _fileHeader;
        private IPayloadMessage _payloadMessage;

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
            var payload = new byte[]{1};
            const int chunkIndex = 0;
            _payloadMessage = new PayloadMessage(broadcastId, payloadIndex, payload,chunkIndex);
            _broadcastMessageConversionService = new BroadcastMessageConversionService();
        }

        [Fact]
        public void Convert_EqualsForwardsAndBackwards()
        {
            //assemble
            var bytes = _broadcastMessageConversionService.Convert(_payloadMessage);

            //act
            var messages = _broadcastMessageConversionService.Convert(bytes);
            var actual = messages.PayloadMessage;

            //assert
            Assert.Equal(_payloadMessage.ChunkIndex, actual.ChunkIndex);
            Assert.True(_payloadMessage.Payload.SequenceEqual(actual.Payload));
            Assert.Equal(_payloadMessage.PayloadIndex, actual.PayloadIndex);
            Assert.Equal(_payloadMessage.BroadcastId, actual.BroadcastId);
        }
    }
}