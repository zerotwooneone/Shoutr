using System;
using System.Linq;
using System.Numerics;
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

        [Fact]
        public void GetFirst_CallsGetBroadcastHeader()
        {
            //act
            _fileBroadcastEnumerationFactory
                .GetEnumeration(It.IsAny<string>())
                .First();

            _fileMessageService.Verify(
                fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Once);
        }

        [Fact]
        public void GetSecond_CallsGetFileHeader()
        {
            //act
            _fileBroadcastEnumerationFactory
                .GetEnumeration(It.IsAny<string>())
                .Skip(1)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Once);
        }

        [Fact]
        public void GetThird_CallsGetChunkHeader()
        {
            //act
            _fileBroadcastEnumerationFactory
                .GetEnumeration(It.IsAny<string>())
                .Skip(2)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetChunkHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>()),
                Times.Once);
        }

        [Fact]
        public void GetForth_CallsGetPayloads()
        {
            //act
            _fileBroadcastEnumerationFactory
                .GetEnumeration(It.IsAny<string>())
                .Skip(3)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>()),
                Times.Once);
        }

        [Fact]
        public void GetFifth_CallsGetChunkHeader()
        {
            //act
            _fileBroadcastEnumerationFactory
                .GetEnumeration(It.IsAny<string>())
                .Skip(4)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetChunkHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetSixth_CallsGetFileHeader()
        {
            //act
            _fileBroadcastEnumerationFactory
                .GetEnumeration(It.IsAny<string>())
                .Skip(5)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetSeventh_CallsGetBroadcastHeader()
        {
            //act
            _fileBroadcastEnumerationFactory
                .GetEnumeration(It.IsAny<string>())
                .Skip(6)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetEnumeration_HasCountOfSeven()
        {
            //assemble
            const int expected = 7;

            //act
            var actual = _fileBroadcastEnumerationFactory
                .GetEnumeration(It.IsAny<string>())
                .Count();

            Assert.Equal(expected, actual);
        }
    }
}