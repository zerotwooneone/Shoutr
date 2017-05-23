using System;
using System.Collections.Generic;
using System.Diagnostics;
using Library.Message;
using Xunit;

namespace Library.Tests.Message
{
    public class BroadcastMessageConversionService_OneByteOneCharFileShould
    {
        private readonly BroadcastMessageConversionService _broadcastMessageConversionService;
        private readonly BroadcastMessage _broadcastMessage;
        
        private static int Compare(BroadcastMessage x, BroadcastMessage y)
        {
            throw new NotImplementedException();
        }

        public BroadcastMessageConversionService_OneByteOneCharFileShould()
        {
            _broadcastMessage = new BroadcastMessage
            {
                PayloadId = null,
                FileName = "a",
                BroadcastId = Guid.NewGuid(),
                ChunkCount = 1,
                ChunkId = null,
                ChunkSizeInBytes = 1,
                IsLast = null,
                Payload = null
            };
            _broadcastMessageConversionService = new BroadcastMessageConversionService();
        }

        [Fact]
        public void Convert_EqualsForwardsAndBackwards()
        {
            //assemble
            var bytes = _broadcastMessageConversionService.Convert(_broadcastMessage);
            
            //act
            var actual = _broadcastMessageConversionService.Convert(bytes);

            //assert
            Assert.Equal(_broadcastMessage, actual);
        }
    }
}