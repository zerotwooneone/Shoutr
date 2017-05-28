using Library.File;
using Library.Message;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileBroadcastEnumerationFactory_OneByteFileShould
    {
        private readonly FileBroadcastEnumerationFactory _fileBroadcastEnumerationFactory;
        private readonly Mock<IFileMessageEnumerationFactory> _mockFileEnumerationFactory;
        private readonly Mock<IBroadcastMessageConversionService> _mockBroadcastMessageConversionService;

        public FileBroadcastEnumerationFactory_OneByteFileShould()
        {
            _mockFileEnumerationFactory = new Mock<IFileMessageEnumerationFactory>();
            _mockBroadcastMessageConversionService = new Mock<IBroadcastMessageConversionService>();
            _fileBroadcastEnumerationFactory = new FileBroadcastEnumerationFactory(_mockFileEnumerationFactory.Object, _mockBroadcastMessageConversionService.Object);
        }

        [Fact]
        public void GetEnumeration_NotNull()
        {
            //act
            var actual =_fileBroadcastEnumerationFactory.GetEnumeration(It.IsAny<string>());

            Assert.NotNull(actual);
        }
    }
}