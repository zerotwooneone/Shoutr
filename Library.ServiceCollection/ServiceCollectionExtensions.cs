using Library.ByteTransfer;
using Library.Interface.ByteTransfer;
using Library.Interface.Listen;
using Library.Interface.Message;
using Library.Listen;
using Library.Message;
using Library.Udp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Library.ServiceCollection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddShoutr(this IServiceCollection services)
        {
            services.TryAddSingleton<IListenerFactory, ListenerFactory>();
            services.TryAddSingleton<IBroadcastMessageConversionService, BroadcastMessageConversionService>();
            services.TryAddSingleton<IByteService, ByteService>();
            services.TryAddSingleton<UdpClientFactory>();
        }
    }
}
