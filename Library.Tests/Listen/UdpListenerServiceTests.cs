using Library.Listen;
using Library.Message;
using Moq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Library.Tests.Listen
{
    public class UdpListenerServiceTests : IDisposable
    {
        private MockRepository mockRepository;

        private IPAddress mockIPAddress;
        private int mockPort;
        private Mock<UdpClientFactory> mockUdpClientFactory;
        private Mock<IBroadcastMessageConversionService> mockBroadcastMessageConversionService;
        private Mock<IUdpListener> _udpListener;

        public UdpListenerServiceTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockIPAddress = IPAddress.Any;
            mockPort = 8088;
            this.mockUdpClientFactory = this.mockRepository.Create<UdpClientFactory>();
            this.mockBroadcastMessageConversionService = this.mockRepository.Create<IBroadcastMessageConversionService>();
            _udpListener = mockRepository.Create<IUdpListener>();
        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        private UdpListenerService CreateService()
        {
            return new UdpListenerService(
                this.mockIPAddress,
                mockPort,
                this.mockUdpClientFactory.Object,
                this.mockBroadcastMessageConversionService.Object);
        }

        [Fact]
        public async Task Subscribe_WhenCold_CreatesClient()
        {
            // Arrange
            SetupUdpClientFactory();
            var service = this.CreateService();
            _udpListener
                .Setup(ul => ul.ReceiveAsync())
                .Returns(Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(t=>CreateUdpReceive()));
            mockBroadcastMessageConversionService
                .Setup(bcs => bcs.Convert(It.IsAny<byte[]>()))
                .Returns(new Messages());
            
            // Act
            var result = await service
                .MessagesObservable
                .FirstOrDefaultAsync();

            // Assert
            mockUdpClientFactory.Verify(ucf=>ucf.Create(It.IsAny<IPEndPoint>()), Times.Once);
        }

        [Fact]
        public async Task Unsubscribe_WhenCold_DisposesClient()
        {
            // Arrange
            SetupUdpClientFactory();
            var service = this.CreateService();
            _udpListener
                .Setup(ul => ul.ReceiveAsync())
                .Returns(Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(t=>CreateUdpReceive()));
            _udpListener
                .Setup(ul => ul.Dispose())
                .Verifiable();
            
            // Act
            service
                .MessagesObservable
                .Subscribe()
                .Dispose();

            // Assert
            _udpListener.Verify(ucf=>ucf.Dispose(), Times.Once);
        }
        
        private static UdpReceiveResult CreateUdpReceive()
        {
            return new UdpReceiveResult(new byte[] {16, 16},
                new IPEndPoint(IPAddress.Any, 7777));
        }

        private void SetupUdpClientFactory()
        {
            mockUdpClientFactory
                .Setup(ucf => ucf.Create(It.IsAny<IPEndPoint>()))
                .Returns(_udpListener.Object);
        }
    }
}
