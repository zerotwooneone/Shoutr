using System;
using System.Linq;
using System.Numerics;
using Library.File;
using Library.Message;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileBroadcastEnumeration_WhenOneByteFile_Should
    {
        private readonly FileBroadcastEnumeration _fileBroadcastEnumeration;
        private readonly Mock<IFileMessageService> _fileMessageService;
        private readonly Mock<IBroadcastMessageConversionService> _mockBroadcastMessageConversionService;
        private const string _fileName = "fileName";
        private readonly Guid _broadCastId = Guid.Parse("{81a130d2-502f-4cf1-a376-63edeb000e9f}");

        public FileBroadcastEnumeration_WhenOneByteFile_Should()
        {
            _fileMessageService = new Mock<IFileMessageService>();
            var fileHeader = new Mock<IFileHeader>();
            fileHeader
                .SetupGet(fh => fh.ChunkCount)
                .Returns(1);
            _fileMessageService
                .Setup(ms => ms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>()))
                .Returns(fileHeader.Object);
            var broadcastHeader = new Mock<IBroadcastHeader>();
            _fileMessageService
                .Setup(ms => ms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>()))
                .Returns(broadcastHeader.Object);
            var chunkHeader = new Mock<IChunkHeader>();
            _fileMessageService
                .Setup(ms => ms.GetChunkHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>(),
                    It.IsAny<bool?>()))
                .Returns(chunkHeader.Object);
            var payload = new Mock<IPayloadMessage>();
            payload
                .SetupGet(pl => pl.Payload)
                .Returns(new byte[] {99});
            var payloads = new[] {payload.Object};
            _fileMessageService
                .Setup(ms => ms.GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>()))
                .Returns(payloads);

            _mockBroadcastMessageConversionService = new Mock<IBroadcastMessageConversionService>();
            _fileBroadcastEnumeration = new FileBroadcastEnumeration(_fileMessageService.Object, _mockBroadcastMessageConversionService.Object, _fileName, _broadCastId);
        }

        [Fact]
        public void GetFirst_CallsGetBroadcastHeader()
        {
            
            //act
            _fileBroadcastEnumeration
                .First();

            _fileMessageService.Verify(
                fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>()),
                Times.Once);
        }

        [Fact]
        public void GetSecond_CallsGetFileHeader()
        {
            
            //act
            _fileBroadcastEnumeration
                .Skip(1)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>()),
                Times.Once);
        }

        [Fact]
        public void GetThird_CallsGetChunkHeader()
        {
            
            //act
            _fileBroadcastEnumeration
                .Skip(2)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetChunkHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>(), It.IsAny<bool?>()),
                Times.Once);
        }

        [Fact]
        public void GetForth_CallsGetPayloads()
        {
            
            //act
            _fileBroadcastEnumeration
                .Skip(3)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>()),
                Times.Once);
        }

        [Fact]
        public void GetFifth_CallsGetChunkHeader()
        {
            
            //act
            _fileBroadcastEnumeration
                .Skip(4)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetChunkHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>(), It.IsAny<bool?>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetSixth_CallsGetFileHeader()
        {
            
            //act
            _fileBroadcastEnumeration
                .Skip(5)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetSeventh_CallsGetBroadcastHeader()
        {
            
            //act
            _fileBroadcastEnumeration
                .Skip(6)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetEnumeration_HasCountOfSeven()
        {
            
            //assemble
            const int expected = 7;

            //act
            var actual = _fileBroadcastEnumeration
                .Count();

            Assert.Equal(expected, actual);
        }
    }
}