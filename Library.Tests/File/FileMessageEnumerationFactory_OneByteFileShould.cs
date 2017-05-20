using System;
using System.Linq;
using Library.File;
using Moq;
using Xunit;

namespace Library.Tests.File
{
    public class FileMessageEnumerationFactory_OneByteFileShould
    {
        private readonly FileMessageEnumerationFactory _fileMessageEnumerationFactory;
        private readonly Mock<IFileDataFactory> _mockFIleDataFactory;

        public FileMessageEnumerationFactory_OneByteFileShould()
        {
            _mockFIleDataFactory = new Mock<IFileDataFactory>();
            _mockFIleDataFactory
                .Setup(ff => ff.Create(It.IsAny<string>()))
                .Returns(new FileData
                {
                    ChunkCount = 1,
                    FileChunks = new[]
                    {
                        new FileChunk
                        {
                            ChunkId = 0,
                            Payloads = new []
                            {
                                new FilePayload
                                {
                                    Bytes = new []{byte.MinValue},
                                    PayloadId = 0
                                }
                            }
                        }
                    },
                    FileName = "FileName"
                });
            _fileMessageEnumerationFactory = new FileMessageEnumerationFactory(_mockFIleDataFactory.Object);
        }

        [Fact]
        public void Create_NotNull()
        {
            var actual = _fileMessageEnumerationFactory.Create(It.IsAny<string>(), It.IsAny<Guid>());

            Assert.NotNull(actual);
        }

        [Fact]
        public void Create_EveryMessageHasBroadcastId()
        {
            var actual = _fileMessageEnumerationFactory
                .Create(It.IsAny<string>(), It.IsAny<Guid>())
                .All(m => m.BroadcastId != Guid.Empty);

            Assert.True(actual);
        }

        [Fact]
        public void Create_FirstMessageHasChunkCount()
        {
            var actual = _fileMessageEnumerationFactory.Create(It.IsAny<string>(), It.IsAny<Guid>())
                .First()
                .ChunkCount;

            Assert.NotNull(actual);
        }

        [Fact]
        public void Create_FirstMessageHasFileName()
        {
            var actual = _fileMessageEnumerationFactory.Create(It.IsAny<string>(), It.IsAny<Guid>())
                .First()
                .FileName;

            Assert.NotNull(actual);
        }

        [Fact]
        public void Create_LastMessageHasFileName()
        {
            var actual = _fileMessageEnumerationFactory.Create(It.IsAny<string>(), It.IsAny<Guid>())
                .Last()
                .FileName;

            Assert.NotNull(actual);
        }

        [Fact]
        public void Create_LastMessageHasIsLast()
        {
            var actual = _fileMessageEnumerationFactory.Create(It.IsAny<string>(), It.IsAny<Guid>())
                .Last()
                .IsLast;

            Assert.NotNull(actual);
            Assert.True(actual.Value);
        }

        [Fact]
        public void Create_LastChunkMessageHasIsLast()
        {
            var actual = _fileMessageEnumerationFactory
                .Create(It.IsAny<string>(), It.IsAny<Guid>())
                .Last(m => m.ChunkId.HasValue).IsLast;

            Assert.NotNull(actual);
            Assert.True(actual.Value);
        }

        [Fact]
        public void Create_LastPayloadMessageHasIsLast()
        {
            var actual = _fileMessageEnumerationFactory
                .Create(It.IsAny<string>(), It.IsAny<Guid>())
                .Last(m => m.PayloadId.HasValue).IsLast;

            Assert.NotNull(actual);
            Assert.True(actual.Value);
        }
    }
}