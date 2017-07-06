using System;
using System.Linq;
using System.Numerics;
using Library.Configuration;
using Library.File;
using Library.Message;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileMessageService_WhenTwoByteFile_OneBytePayload_Should
    {
        private readonly FileMessageService _fileMessageService;
        private readonly Mock<IFileDataRepository> _mockFIleDataRepository;
        private readonly Mock<IConfigurationService> _configurationService;

        public FileMessageService_WhenTwoByteFile_OneBytePayload_Should()
        {
            _configurationService = new Mock<IConfigurationService>();
            const int payloadSize = 1;
            long byteCount = 2;
            uint pageSize = (uint)byteCount;
            _configurationService
                .SetupGet(cs => cs.PayloadSizeInBytes)
                .Returns(payloadSize);
            _configurationService
                .SetupGet(cs => cs.PageSize)
                .Returns(pageSize);
            _mockFIleDataRepository = new Mock<IFileDataRepository>();

            _mockFIleDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(byteCount);
            _mockFIleDataRepository
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<ushort>(), It.IsAny<BigInteger>()))
                .Returns(new[] { It.IsAny<byte>(), It.IsAny<byte>() });
            _fileMessageService = new FileMessageService(_mockFIleDataRepository.Object, _configurationService.Object);
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
    }
}