using System;
using System.Numerics;
using Library.Configuration;
using Library.File;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileMessageServiceTests : IDisposable
    {
        private readonly Mock<IFileDataRepository> _mockFileDataRepository;
        private readonly Mock<IConfigurationService> _configurationService;
        private readonly MockRepository _mockRepository;
        private readonly Mock<IFileMessageConfig> _mockFileMessageConfig;
        const long byteCount = 1;
        
        public FileMessageServiceTests() 
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _configurationService = _mockRepository.Create<IConfigurationService>();
            _mockFileDataRepository = _mockRepository.Create<IFileDataRepository>();
            _mockFileMessageConfig = _mockRepository.Create<IFileMessageConfig>();

        }

        private FileMessageService GetService()
        {
            return new FileMessageService(_mockFileDataRepository.Object, _configurationService.Object);
        }

        public void Dispose()
        {
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void GetFileHeader_WhenFirstPayloadIndex_ReturnFirstPayloadIndexOfOne()
        {
            //assemble
            var fileMessageService = GetService();
            long expected = 1;

            //act
            var actual = fileMessageService.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), expected).FirstPayloadIndex;
            
            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBroadcastHeader_ReturnOneByteMaxPayloadSize_WhenOneByteFile()
        {
            //assemble
            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(999);

            var fileMessageService = GetService();

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(byteCount);
            
            //act
            var actual = fileMessageService.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(),_mockFileMessageConfig.Object).MaxPayloadSizeInBytes;
            const long expected = 1;

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBroadcastHeader_ReturnOneByteMaxPayloadSize_WhenOneByteMaxPayloadSize()
        {
            //assemble
            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(1);

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(999);
            
            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(byteCount);
            
            var fileMessageService = GetService();

            //act
            var actual = fileMessageService.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(),_mockFileMessageConfig.Object).MaxPayloadSizeInBytes;
            const long expected = byteCount;
            
            //assert
            Assert.Equal(expected, actual);
        }
    }
}