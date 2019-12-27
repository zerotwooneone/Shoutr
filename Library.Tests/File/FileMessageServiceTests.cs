using Library.File;
using Moq;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Library.Interface.Configuration;
using Library.Interface.File;
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

        [Fact]
        public async Task GetPayloads_ReturnsFirstIndex5_WhenStartIndexIs5()
        {
            //assemble
            var fileMessageService = GetService();
            long expected = 5;

            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(999);

            _configurationService
                .SetupGet(cs=>cs.PageSize)
                .Returns(999);

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(1);

            _mockFileDataRepository
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileReadResult(new byte[]{ 99 }));
                        
            //act
            var actual = await fileMessageService
                .GetPayloads(It.IsAny<string>(), It.IsAny<Guid>(), _mockFileMessageConfig.Object, startingPayloadIndex: expected)
                .Take(1);

            //assert
            Assert.Equal(expected, actual.PayloadIndex);
        }

        [Fact]
        public void GetPayloads_DoesNotCallGetPage_WhenCalled()
        {
            //assemble
            var fileMessageService = GetService();

            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(999);

            _configurationService
                .SetupGet(cs=>cs.PageSize)
                .Returns(999);

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(1);
                        
            //act
            var actual = fileMessageService
                .GetPayloads(It.IsAny<string>(), It.IsAny<Guid>(), _mockFileMessageConfig.Object);

            //assert
            _mockFileDataRepository
                .Verify(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetPayloads_CallsGetPageTwice_AfterAwaitingWhen2ByteFileAnd1BytePageSize()
        {
            //assemble
            var fileMessageService = GetService();

            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(999);

            _configurationService
                .SetupGet(cs=>cs.PageSize)
                .Returns(1);

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(2);

            _mockFileDataRepository
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileReadResult(new byte[]{ 99 }));
                        
            //act
            var actual = await fileMessageService
                .GetPayloads(It.IsAny<string>(), It.IsAny<Guid>(), _mockFileMessageConfig.Object);

            //assert
            _mockFileDataRepository
                .Verify(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task GetPayloads_CallsGetPageOnce_AfterAwaitingOne()
        {
            //assemble
            var fileMessageService = GetService();

            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(999);

            _configurationService
                .SetupGet(cs=>cs.PageSize)
                .Returns(1);

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(2);

            _mockFileDataRepository
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileReadResult(new byte[]{ 99 }));
                        
            //act
            var actual = await fileMessageService
                .GetPayloads(It.IsAny<string>(), It.IsAny<Guid>(), _mockFileMessageConfig.Object)
                .Take(1);

            //assert
            _mockFileDataRepository
                .Verify(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPayloads_FirstPayloadByteFromStartingBytes_AfterAwaitingFirstPayload()
        {
            //assemble
            var fileMessageService = GetService();

            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(999);

            _configurationService
                .SetupGet(cs=>cs.PageSize)
                .Returns(999);

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(6);

            _mockFileDataRepository
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileReadResult(new byte[]{ 99, 100, 101, 102, 103, 104 }));
    
            const byte expected = 55;

            //act
            var actual = (await fileMessageService
                .GetPayloads(It.IsAny<string>(), It.IsAny<Guid>(), _mockFileMessageConfig.Object, startingBytes: new byte[]{ expected })
                .Take(1))
                .Payload[0];

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetPayloads_SecondByteIsTheFirstByteFromFile_AfterAwaitingFirstPayload()
        {
            //assemble
            var fileMessageService = GetService();

            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.MaxPayloadSizeInBytes)
                .Returns(999);

            _configurationService
                .SetupGet(cs=>cs.PageSize)
                .Returns(999);

            _mockFileDataRepository
                .Setup(fr => fr.GetByteCount(It.IsAny<string>()))
                .Returns(6);

            const byte expected = 55;

            _mockFileDataRepository
                .Setup(fr => fr.GetPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileReadResult(new byte[]{ expected, 100, 101, 102, 103, 104 }));

            //act
            var actual = (await fileMessageService
                .GetPayloads(It.IsAny<string>(), It.IsAny<Guid>(), _mockFileMessageConfig.Object, startingBytes: new byte[]{ 11 })
                .Take(1))
                .Payload[1];

            //assert
            Assert.Equal(expected, actual);
        }
    }
}