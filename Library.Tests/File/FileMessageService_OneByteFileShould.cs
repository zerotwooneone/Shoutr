using System;
using System.Linq;
using System.Numerics;
using Library.Configuration;
using Library.File;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileMessageService_WhenOneByteFileShould
    {
        private readonly FileMessageService _fileMessageService;
        private readonly Mock<IFileDataRepository> _mockFileDataRepository;
        private readonly Mock<IConfigurationService> _configurationService;

        public FileMessageService_WhenOneByteFileShould()
        {
            _configurationService = new Mock<IConfigurationService>();
            _mockFileDataRepository = new Mock<IFileDataRepository>();
            BigInteger byteCount=1;
            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(byteCount);
            _mockFileDataRepository
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<ushort>(), It.IsAny<BigInteger>()))
                .Returns(new[] {byte.MinValue});
            _fileMessageService = new FileMessageService(_mockFileDataRepository.Object, _configurationService.Object);
        }

        [Fact]
        public void GetFileHeader_ReturnChunkCountOfOne()
        {
            var actual = _fileMessageService.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>()).ChunkCount;
            BigInteger expected=1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBroadcastHeader_ReturnOneByteChunkSize()
        {
            var actual = _fileMessageService.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>()).ChunkSizeInBytes;
            const ushort expected = 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPayloadByChunkIndex_ReturnOnePayload()
        {
            BigInteger chunkIndex = 0;
            var actual = _fileMessageService
                .GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), chunkIndex)
                .Count();
            int expected = 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPayloadByChunkIndex_ReturnPayloadWithOneByte()
        {
            BigInteger chunkIndex = 0;
            var actual = _fileMessageService
                .GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), chunkIndex)
                .First()
                .Payload
                .Length;
            int expected = 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetFileHeader_ReturnLazyLoadedPayloads()
        {
           _fileMessageService
                .GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>());

            _mockFileDataRepository
                .Verify(dr => dr.GetPage(It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<BigInteger>()),
                    Times.Never);
        }
    }
}