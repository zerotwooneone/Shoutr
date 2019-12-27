using Library.ByteTransfer;
using Library.Interface.ByteTransfer;
using Library.Interface.Listen;
using Library.Interface.Message;
using Library.Listen;
using Library.Message;
using Library.Udp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shoutr
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IListenerFactory, ListenerFactory>()
                .AddSingleton<IBroadcastMessageConversionService, BroadcastMessageConversionService>()
                .AddSingleton<IByteService, ByteService>()
                .AddSingleton<UdpClientFactory>();
        }
    }
}