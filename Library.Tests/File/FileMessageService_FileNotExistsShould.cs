using System;
using System.IO;
using Library.Configuration;
using Library.File;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileMessageService_FileNotExistsShould
    {
        private readonly FileMessageService _fileMessageService;
        private readonly Mock<IFileDataRepository> _mockFIleDataFactory;
        private Mock<IConfigurationService> _configurationService;

        public FileMessageService_FileNotExistsShould()
        {
            _mockFIleDataFactory = new Mock<IFileDataRepository>();
            _configurationService = new Mock<IConfigurationService>();
            _fileMessageService = new FileMessageService(_mockFIleDataFactory.Object, _configurationService.Object);
        }

        [Fact]
        public void GetEnumeration_Throws()
        {
            Assert.Throws<FileNotFoundException>(
                () => _fileMessageService.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>()));
        }
    }
}