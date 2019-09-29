﻿using Library.ByteTransfer;
using Library.Listen;
using Library.Message;
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