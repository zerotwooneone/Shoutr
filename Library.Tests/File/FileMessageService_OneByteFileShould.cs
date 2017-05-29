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
        private readonly Mock<IFileDataRepository> _mockFIleDataFactory;
        private readonly Mock<IConfigurationService> _configurationService;

        public FileMessageService_WhenOneByteFileShould()
        {
            _configurationService = new Mock<IConfigurationService>();
            _mockFIleDataFactory = new Mock<IFileDataRepository>();
            BigInteger byteCount=1;
            _mockFIleDataFactory
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(byteCount);
            _mockFIleDataFactory
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<ushort>(), It.IsAny<BigInteger>()))
                .Returns(new[] {byte.MinValue});
            _fileMessageService = new FileMessageService(_mockFIleDataFactory.Object, _configurationService.Object);
        }

        [Fact]
        public void GetFileHeader_ReturnChunkCountOfOne()
        {
            string ingnoredFileName="filename";
            Guid ignoredId = Guid.Empty;
            var actual = _fileMessageService.GetFileHeader(ingnoredFileName,ignoredId).ChunkCount;
            BigInteger expected=1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPayloadByChunkIndex_ReturnOnePayload()
        {
            string ingnoredFileName = "filename";
            Guid ignoredId = Guid.Empty;
            BigInteger chunkIndex = 0;
            var actual = _fileMessageService
                .GetPayloadsByChunkIndex(ingnoredFileName, ignoredId, chunkIndex)
                .Count();
            int expected = 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPayloadByChunkIndex_ReturnPayloadWithOneByte()
        {
            string ingnoredFileName = "filename";
            Guid ignoredId = Guid.Empty;
            BigInteger chunkIndex = 0;
            var actual = _fileMessageService
                .GetPayloadsByChunkIndex(ingnoredFileName, ignoredId, chunkIndex)
                .First()
                .Payload
                .Length;
            int expected = 1;

            Assert.Equal(expected, actual);
        }
    }
}