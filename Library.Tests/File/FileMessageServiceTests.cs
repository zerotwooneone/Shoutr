using System;
using System.Linq;
using System.Numerics;
using Library.Configuration;
using Library.File;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileMessageServiceTests
    {
        private readonly FileMessageService _fileMessageService;
        private readonly Mock<IFileDataRepository> _mockFileDataRepository;
        private readonly Mock<IConfigurationService> _configurationService;
        private readonly MockRepository _mockRepository;
        private readonly Mock<IFileMessageConfig> _mockFileMessageConfig;
        const long byteCount = 1;

        public FileMessageServiceTests()
        {
            _configurationService = new Mock<IConfigurationService>();
            _configurationService
                .SetupGet(cs => cs.PageSize)
                .Returns(8000);
            _configurationService
                .SetupGet(cs => cs.PayloadSizeInBytes)
                .Returns(1000);

            _mockFileDataRepository = new Mock<IFileDataRepository>();

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(byteCount);
            _mockFileDataRepository
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<BigInteger>()))
                .Returns(new[] { byte.MinValue });

            _mockRepository = new MockRepository(MockBehavior.Strict);
            _mockFileMessageConfig = _mockRepository.Create<IFileMessageConfig>();

            _fileMessageService = new FileMessageService(_mockFileDataRepository.Object, _configurationService.Object);
        }

        [Fact]
        public void GetFileHeader_WhenFirstPayloadIndex_ReturnFirstPayloadIndexOfOne()
        {
            long expected = 1;
            var actual = _fileMessageService.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), expected).FirstPayloadIndex;
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBroadcastHeader_ReturnOneByteMaxPayloadSize_WhenOneByteFile()
        {
            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(999);

            var actual = _fileMessageService.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(),_mockFileMessageConfig.Object).MaxPayloadSizeInBytes;
            const long expected = 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBroadcastHeader_ReturnOneByteMaxPayloadSize_WhenOneByteMaxPayloadSize()
        {
            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(1);

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(999);

            var actual = _fileMessageService.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(),_mockFileMessageConfig.Object).MaxPayloadSizeInBytes;
            const long expected = byteCount;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPayloadByChunkIndex_ReturnOnePayload_WhenOneByteFile()
        {
            BigInteger chunkIndex = 0;
            var actual = _fileMessageService
                .GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), chunkIndex)
                .Count();
            int expected = 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPayloadByChunkIndex_ReturnPayloadWithOneByte_WhenOneByteFile()
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
        public void GetPayloadsByChunkIndex_ReturnLazyLoadedPayloads_WhenOneByteFile()
        {
            _fileMessageService
                 .GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>());

            _mockFileDataRepository
                .Verify(dr => dr.GetPage(It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<BigInteger>()),
                    Times.Never);
        }

        [Fact]
        public void GetPayloadByChunkIndex_ReturnPayloadWithOneByte_WhenTwoByteFile()
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