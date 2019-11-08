using Library.File;
using Library.Message;
using Library.Reactive;
using Library.Tests.Reactive;
using Microsoft.Reactive.Testing;
using Moq;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace Library.Tests.File
{
    public class MessageObservableFactoryTests : IDisposable
    {
        private MockRepository mockRepository;

        private Mock<IFileMessageService> mockFileMessageService;
        private readonly Mock<IFileMessageConfig> mockFileMessageConfig;
        private readonly Mock<ISchedulerProvider> mockSchedulerProvider;
        private readonly TestScheduler _testScheduler;

        public MessageObservableFactoryTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockFileMessageService = this.mockRepository.Create<IFileMessageService>();
            mockFileMessageConfig = mockRepository.Create<IFileMessageConfig>();  
            mockSchedulerProvider = mockRepository.Create<ISchedulerProvider>();
            _testScheduler = new TestScheduler();
        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        private MessageObservableFactory CreateFactory()
        {
            return new MessageObservableFactory(
                this.mockFileMessageService.Object,
                mockSchedulerProvider.Object);
        }

        [Fact]
        public async Task GetBroadcastHeaderObservable_NotIsLast_AfterFirstEmission()
        {
            // Arrange
            var factory = this.CreateFactory();
            string fileName = null;

            mockFileMessageService
                .Setup(fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<bool?>()))
                .Returns(new BroadcastHeader(Guid.Empty, false, 1));
            
            SetupDefaultScheduler();

            mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            // Act
            var actual = await factory.GetBroadcastHeaderObservable(
                fileName,
                mockFileMessageConfig.Object,
                Observable.Never<object>(),
                Guid.Empty)
                .FirstOrDefaultAsync();

            // Assert
            Assert.False(actual.IsLast);
        }        

        [Fact]
        public async Task GetBroadcastHeaderObservable_Rebroadcasts_AfterRebroadcastInterval()
        {
            // Arrange
            var factory = this.CreateFactory();
            
            mockFileMessageService
                .Setup(fms=>fms.GetBroadcastHeader(It.IsAny<string>(),It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<bool?>()))
                .Returns(new BroadcastHeader(Guid.Empty, false, 1));

            SetupDefaultScheduler();

            var rebroadcastInterval = TimeSpan.FromTicks(1);
            mockFileMessageConfig
                .SetupGet(fmc=>fmc.HeaderRebroadcastInterval)
                .Returns(rebroadcastInterval);
            
            // Act
            var actual = await factory.GetBroadcastHeaderObservable(
                fileName: null,
                mockFileMessageConfig.Object,
                Observable.Never<object>(),
                broadcastId: Guid.Empty)
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

            mockFileMessageService
                .Setup(fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.Is<bool?>(b=>b == null || !b.HasValue)))
                .Returns(new BroadcastHeader(Guid.Empty, false, 1));
            
            mockFileMessageService
                .Setup(fms => fms.GetBroadcastHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.Is<bool?>(b=>b.HasValue && b.Value)))
                .Returns(new BroadcastHeader(Guid.Empty, true, 1));

            SetupDefaultScheduler();

            mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            var completedObservable = Observable.Empty<object>();
            
            // Act
            var actual = await factory.GetBroadcastHeaderObservable(
                fileName,
                mockFileMessageConfig.Object,
                completedObservable,
                Guid.Empty)
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

            mockFileMessageService
                .Setup(fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.IsAny<bool?>()))
                .Returns(new FileHeader(Guid.Empty, false, null, 1));
            
            SetupDefaultScheduler();

            mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            // Act
            var actual = await factory.GetFileHeaderObservable(
                fileName,
                mockFileMessageConfig.Object,
                Observable.Never<object>(),
                Guid.Empty)
                .FirstOrDefaultAsync();

            // Assert
            Assert.False(actual.IsLast);
        }        

        [Fact]
        public async Task GetFileHeaderObservable_Rebroadcasts_AfterRebroadcastInterval()
        {
            // Arrange
            var factory = this.CreateFactory();
            
            mockFileMessageService
                .Setup(fms=>fms.GetFileHeader(It.IsAny<string>(),It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.IsAny<bool?>()))
                .Returns(new FileHeader(Guid.Empty, false, null, 1));

            SetupDefaultScheduler();

            var rebroadcastInterval = TimeSpan.FromTicks(1);
            mockFileMessageConfig
                .SetupGet(fmc=>fmc.HeaderRebroadcastInterval)
                .Returns(rebroadcastInterval);
            
            // Act
            var actual = await factory.GetFileHeaderObservable(
                fileName: null,
                mockFileMessageConfig.Object,
                Observable.Never<object>(),
                broadcastId: Guid.Empty)
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

            mockFileMessageService
                .Setup(fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.Is<bool?>(b=>b == null || !b.HasValue)))
                .Returns(new FileHeader(Guid.Empty, false, null, 1));
            
            mockFileMessageService
                .Setup(fms => fms.GetFileHeader(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IFileMessageConfig>(), It.IsAny<long>(), It.Is<bool?>(b=>b.HasValue && b.Value)))
                .Returns(new FileHeader(Guid.Empty, true, null, 1));

            SetupDefaultScheduler();

            mockFileMessageConfig
                .SetupGet(fmc => fmc.HeaderRebroadcastInterval)
                .Returns(TimeSpan.MaxValue);

            var completedObservable = Observable.Empty<object>();
            
            // Act
            var actual = await factory.GetFileHeaderObservable(
                fileName,
                mockFileMessageConfig.Object,
                completedObservable,
                Guid.Empty)
                .LastOrDefaultAsync();

            // Assert
            Assert.True(actual.IsLast);
        }

        private void SetupDefaultScheduler()
        {
            mockSchedulerProvider
                            .SetupGet(sp => sp.Default)
                            .Returns(_testScheduler);
        }
    }
}
