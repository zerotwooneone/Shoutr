using Library.File;
using Library.Message;
using Moq;
using System.IO;
using Xunit;

namespace Library.Tests.File
{
    public class FileBroadcastEnumerationFactoryTests2
    {
        private readonly FileBroadcastEnumerationFactory _fileBroadcastEnumerationFactory;
        private readonly Mock<IFileMessageService> _fileMessageService;
        private readonly Mock<IBroadcastMessageConversionService> _mockBroadcastMessageConversionService;

        public FileBroadcastEnumerationFactoryTests2()
        {
            _fileMessageService = new Mock<IFileMessageService>();
            _mockBroadcastMessageConversionService = new Mock<IBroadcastMessageConversionService>();
            _fileBroadcastEnumerationFactory = new FileBroadcastEnumerationFactory(_fileMessageService.Object, _mockBroadcastMessageConversionService.Object);
        }

        [Fact]
        public void GetEnumeration_WhenOneByteFile_NotNull()
        {
            //act
            var actual = _fileBroadcastEnumerationFactory.GetEnumeration(It.IsAny<string>());

            Assert.NotNull(actual);
        }

        [Fact]
        public void GetEnumeration_FileNotExists_Throws()
        {
            Assert.Throws<FileNotFoundException>(
                () => _fileBroadcastEnumerationFactory.GetEnumeration(It.IsAny<string>()));
        }
    }
}