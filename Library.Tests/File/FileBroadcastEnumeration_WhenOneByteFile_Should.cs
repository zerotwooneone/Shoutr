using System;
using System.Linq;
using System.Numerics;
using Library.File;
using Library.Message;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileBroadcastEnumeration_WhenOneByteFile_Should
    {
        private readonly FileBroadcastEnumeration _fileBroadcastEnumeration;
        private readonly Mock<IFileMessageService> _fileMessageService;
        private readonly Mock<IBroadcastMessageConversionService> _mockBroadcastMessageConversionService;
        private const string _fileName = "fileName";

        public FileBroadcastEnumeration_WhenOneByteFile_Should()
        {
            _fileMessageService = new Mock<IFileMessageService>();
            _mockBroadcastMessageConversionService = new Mock<IBroadcastMessageConversionService>();
            _fileBroadcastEnumeration = new FileBroadcastEnumeration(_fileMessageService.Object, _mockBroadcastMessageConversionService.Object, _fileName);
        }

        [Fact]
        public void GetFirst_CallsGetBroadcastHeader()
        {
            //assemble
            _fileBroadcastEnumeration.GetEnumerator().Reset();

            //act
            _fileBroadcastEnumeration
                .First();

            _fileMessageService.Verify(
                fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Once);
        }

        [Fact]
        public void GetSecond_CallsGetFileHeader()
        {
            //assemble
            _fileBroadcastEnumeration.GetEnumerator().Reset();

            //act
            _fileBroadcastEnumeration
                .Skip(1)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Once);
        }

        [Fact]
        public void GetThird_CallsGetChunkHeader()
        {
            //assemble
            _fileBroadcastEnumeration.GetEnumerator().Reset();

            //act
            _fileBroadcastEnumeration
                .Skip(2)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetChunkHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>()),
                Times.Once);
        }

        [Fact]
        public void GetForth_CallsGetPayloads()
        {
            //assemble
            _fileBroadcastEnumeration.GetEnumerator().Reset();

            //act
            _fileBroadcastEnumeration
                .Skip(3)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetPayloadsByChunkIndex(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>()),
                Times.Once);
        }

        [Fact]
        public void GetFifth_CallsGetChunkHeader()
        {
            //assemble
            _fileBroadcastEnumeration.GetEnumerator().Reset();

            //act
            _fileBroadcastEnumeration
                .Skip(4)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetChunkHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<BigInteger>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetSixth_CallsGetFileHeader()
        {
            //assemble
            _fileBroadcastEnumeration.GetEnumerator().Reset();

            //act
            _fileBroadcastEnumeration
                .Skip(5)
                .First();

            _fileMessageService.Verify(
                fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetSeventh_CallsGetBroadcastHeader()
        {
            //assemble
            _fileBroadcastEnumeration.GetEnumerator().Reset();

            //act
            _fileBroadcastEnumeration
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
            _fileBroadcastEnumeration.GetEnumerator().Reset();

            //assemble
            const int expected = 7;

            //act
            var actual = _fileBroadcastEnumeration
                .Count();

            Assert.Equal(expected, actual);
        }
    }
}