using System.IO;
using Library.File;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileBroadcastEnumerationFactory_FileNotExistsShould
    {
        private readonly FileBroadcastEnumerationFactory _fileBroadcastEnumerationFactory;
        private readonly Mock<IFileMessageEnumerationFactory> _mockFileEnumerationFactory;
        private readonly Mock<IBroadcastMessageConversionService> _mockBroadcastMessageConversionService;

        public FileBroadcastEnumerationFactory_FileNotExistsShould()
        {
            _mockFileEnumerationFactory = new Mock<IFileMessageEnumerationFactory>();
            _mockBroadcastMessageConversionService = new Mock<IBroadcastMessageConversionService>();
            _fileBroadcastEnumerationFactory = new FileBroadcastEnumerationFactory(_mockFileEnumerationFactory.Object, _mockBroadcastMessageConversionService.Object);
        }

        [Fact]
        public void GetEnumeration_Throws()
        {
            Assert.Throws<FileNotFoundException>(
                () => _fileBroadcastEnumerationFactory.GetEnumeration(It.IsAny<string>()));
        }
    }
}