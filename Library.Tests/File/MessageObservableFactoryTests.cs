using Library.File;
using Moq;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Library.Interface.File;
using Library.Interface.Message;
using Xunit;

namespace Library.Tests.File
{
    public class MessageObservableFactoryTests : IDisposable
    {
        private MockRepository mockRepository;

        private Mock<IFileMessageService> _mockFileMessageService;
        private readonly Mock<IFileMessageConfig> _mockFileMessageConfig;
        private readonly TestScheduler _testScheduler;

        public MessageObservableFactoryTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this._mockFileMessageService = this.mockRepository.Create<IFileMessageService>();
            _mockFileMessageConfig = mockRepository.Create<IFileMessageConfig>();
            _testScheduler = new TestScheduler();
        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        private MessageObservableFactory CreateFactory()
        {
            return new MessageObservableFactory(
                this._mockFileMessageService.Object);
        }

        [Fact]
        public async Task GetBroadcastHeaderObservable_NotIsLast_AfterFirstEmission()
        {
            // Arrange
            var factory = this.CreateFactory();
            string fileName = null;

            _mockFileMessageService
                .Setup(fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<bool?>()))
                .Returns(new BroadcastHeader(Guid.Empty, false, 1));
            
            _mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            // Act
            var actual = await factory.GetBroadcastHeaderObservable(
                fileName,
                _mockFileMessageConfig.Object,
                Observable.Never<object>(),
                Guid.Empty,
                _testScheduler)
                .FirstOrDefaultAsync();

            // Assert
            Assert.False(actual.IsLast);
        }        

        [Fact]
        public async Task GetBroadcastHeaderObservable_Rebroadcasts_AfterRebroadcastInterval()
        {
            // Arrange
            var factory = this.CreateFactory();
            
            _mockFileMessageService
                .Setup(fms=>fms.GetBroadcastHeader(It.IsAny<string>(),It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<bool?>()))
                .Returns(new BroadcastHeader(Guid.Empty, false, 1));

            var rebroadcastInterval = TimeSpan.FromTicks(1);
            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.HeaderRebroadcastInterval)
                .Returns(rebroadcastInterval);
            
            // Act
            var actual = await factory.GetBroadcastHeaderObservable(
                fileName: null,
                _mockFileMessageConfig.Object,
                Observable.Never<object>(),
                broadcastId: Guid.Empty,
                _testScheduler)
                .Do(m=>{ 
                    if(!_testScheduler.IsEnabled)
                        _testScheduler.AdvanceBy(rebroadcastInterval);
                    })
                .Skip(1)
                .Take(1);
                        
            // Assert
            Assert.False(actual.IsLast);
        }

        [Fact]
        public async Task GetBroadcastHeaderObservable_IsLast_WhenSourceCompleted()
        {
            // Arrange
            var factory = this.CreateFactory();
            string fileName = null;

            _mockFileMessageService
                .Setup(fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.Is<bool?>(b=>b == null || !b.HasValue)))
                .Returns(new BroadcastHeader(Guid.Empty, false, 1));
            
            _mockFileMessageService
                .Setup(fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.Is<bool?>(b=>b.HasValue && b.Value)))
                .Returns(new BroadcastHeader(Guid.Empty, true, 1));

            _mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            var completedObservable = Observable.Empty<object>();
            
            // Act
            var actual = await factory.GetBroadcastHeaderObservable(
                fileName,
                _mockFileMessageConfig.Object,
                completedObservable,
                Guid.Empty,
                _testScheduler)
                .LastOrDefaultAsync();

            // Assert
            Assert.True(actual.IsLast);
        }

        [Fact]
        public async Task GetFileHeaderObservable_NotIsLast_AfterFirstEmission()
        {
            // Arrange
            var factory = this.CreateFactory();
            string fileName = null;

            _mockFileMessageService
                .Setup(fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.IsAny<bool?>()))
                .Returns(new FileHeader(Guid.Empty, false, null, 1));
            
            _mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            // Act
            var actual = await factory.GetFileHeaderObservable(
                fileName,
                _mockFileMessageConfig.Object,
                Observable.Never<object>(),
                Guid.Empty,
                _testScheduler)
                .FirstOrDefaultAsync();

            // Assert
            Assert.False(actual.IsLast);
        }        

        [Fact]
        public async Task GetFileHeaderObservable_Rebroadcasts_AfterRebroadcastInterval()
        {
            // Arrange
            var factory = this.CreateFactory();
            
            _mockFileMessageService
                .Setup(fms=>fms.GetFileHeader(It.IsAny<string>(),It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.IsAny<bool?>()))
                .Returns(new FileHeader(Guid.Empty, false, null, 1));

            var rebroadcastInterval = TimeSpan.FromTicks(1);
            _mockFileMessageConfig
                .SetupGet(fmc=>fmc.HeaderRebroadcastInterval)
                .Returns(rebroadcastInterval);
            
            // Act
            var actual = await factory.GetFileHeaderObservable(
                fileName: null,
                _mockFileMessageConfig.Object,
                Observable.Never<object>(),
                broadcastId: Guid.Empty,
                _testScheduler)
                .Do(m=>{ 
                    if(!_testScheduler.IsEnabled)
                        _testScheduler.AdvanceBy(rebroadcastInterval);
                    })
                .Skip(1)
                .Take(1);
                        
            // Assert
            Assert.False(actual.IsLast);
        }

        [Fact]
        public async Task GetFileHeaderObservable_IsLast_WhenSourceCompleted()
        {
            // Arrange
            var factory = this.CreateFactory();
            string fileName = null;

            _mockFileMessageService
                .Setup(fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.Is<bool?>(b=>b == null || !b.HasValue)))
                .Returns(new FileHeader(Guid.Empty, false, null, 1));
            
            _mockFileMessageService
                .Setup(fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.Is<bool?>(b=>b.HasValue && b.Value)))
                .Returns(new FileHeader(Guid.Empty, true, null, 1));

            _mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            var completedObservable = Observable.Empty<object>();
            
            // Act
            var actual = await factory.GetFileHeaderObservable(
                fileName,
                _mockFileMessageConfig.Object,
                completedObservable,
                Guid.Empty,
                _testScheduler)
                .LastOrDefaultAsync();

            // Assert
            Assert.True(actual.IsLast);
        }

        [Fact]
        public void GetFileBroadcast_HasMessages_BeforeWaiting()
        {
            // Arrange
            var factory = this.CreateFactory();
            
            _mockFileMessageService
                .Setup(fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<bool?>()))
                .Returns(new BroadcastHeader(Guid.Empty, false, 1));
            
            _mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            _mockFileMessageService
                .Setup(fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.IsAny<bool?>()))
                .Returns(new FileHeader(Guid.Empty, false, null, 1));
            
            _mockFileMessageService
                .Setup(fms=>fms.GetPayloads(It.IsAny<string>(), It.IsAny<Guid>(),It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.IsAny<byte[]>()))
                .Returns(Observable.Return<IPayloadMessage>(new PayloadMessage(Guid.Empty, 0, new byte[]{ 99 }, 0)));

            var messages = new List<IMessages>();

            // Act
            factory.GetFileBroadcast(
                "file name",
                _mockFileMessageConfig.Object,
                _testScheduler)
                .Take(1)
                .Subscribe(m=>{
                    messages.Add(m);
                });

            _testScheduler.Start();

            // Assert
            Assert.True(messages.Count > 0);
        }
    }
}
