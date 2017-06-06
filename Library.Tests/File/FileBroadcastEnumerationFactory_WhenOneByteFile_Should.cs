using Library.File;
using Library.Message;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileBroadcastEnumerationFactory_WhenOneByteFile_Should
    {
        private readonly FileBroadcastEnumerationFactory _fileBroadcastEnumerationFactory;
        private readonly Mock<IFileMessageService> _fileMessageService;
        private readonly Mock<IBroadcastMessageConversionService> _mockBroadcastMessageConversionService;

        public FileBroadcastEnumerationFactory_WhenOneByteFile_Should()
        {
            _fileMessageService = new Mock<IFileMessageService>();
            _mockBroadcastMessageConversionService = new Mock<IBroadcastMessageConversionService>();
            _fileBroadcastEnumerationFactory = new FileBroadcastEnumerationFactory(_fileMessageService.Object, _mockBroadcastMessageConversionService.Object);
        }

        [Fact]
        public void GetEnumeration_NotNull()
        {
            //act
            var actual = _fileBroadcastEnumerationFactory.GetEnumeration(It.IsAny<string>());

            Assert.NotNull(actual);
        }
    }
}