using System;
using System.IO;
using Library.File;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileMessageEnumerationFactory_FileNotExistsShould
    {
        private readonly FileMessageEnumerationFactory _fileMessageEnumerationFactory;
        private readonly Mock<IFileDataFactory> _mockFIleDataFactory;

        public FileMessageEnumerationFactory_FileNotExistsShould()
        {
            _mockFIleDataFactory = new Mock<IFileDataFactory>();
            _fileMessageEnumerationFactory = new FileMessageEnumerationFactory(_mockFIleDataFactory.Object);
        }

        [Fact]
        public void GetEnumeration_Throws()
        {
            Assert.Throws<FileNotFoundException>(
                () => _fileMessageEnumerationFactory.Create(It.IsAny<string>(), It.IsAny<Guid>()));
        }
    }
}