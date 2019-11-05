using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Library.Listen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Shoutr
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .AddCommandLine(args);

            IConfiguration config = builder
                .Build();

            ServiceCollection serviceCollection = new ServiceCollection();
            
            var startup = new Startup(config);
            startup.ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var listenerFactory = serviceProvider.GetRequiredService<IListenerFactory>();

            var program = new Program(listenerFactory, 
                config);

            program.Run().Wait();
        }

        private async Task Run()
        {
            Console.WriteLine($"Listening on port:{_configuration["listen"]}...");

            var listener = _listenerFactory.CreateBroadcastListener(int.Parse(_configuration["listen"]));
            var messages = await listener
                .MessagesObservable
                .FirstOrDefaultAsync();

            Console.WriteLine($"Received {JsonConvert.SerializeObject(messages)}");
        }
        
        private readonly IListenerFactory _listenerFactory;
        private readonly IConfiguration _configuration;

        public Program(IListenerFactory listenerFactory,
            IConfiguration configuration)
        {
            _listenerFactory = listenerFactory;
            _configuration = configuration;
        }

        public static class RandomGen3
{
    private static readonly RNGCryptoServiceProvider _global =
        new RNGCryptoServiceProvider();
    [ThreadStatic]
    private static Random _local;

    public static int Next()
    {
        Random inst = _local;
        if (inst == null)
        {
            byte[] buffer = new byte[4];
            _global.GetBytes(buffer);
            _local = inst = new Random(
                BitConverter.ToInt32(buffer, 0));
        }
        return inst.Next();
    }

            public static int Next(int minValue, int maxValue)
    {
        Random inst = _local;
        if (inst == null)
        {
            byte[] buffer = new byte[4];
            _global.GetBytes(buffer);
            _local = inst = new Random(
                BitConverter.ToInt32(buffer, 0));
        }
        return inst.Next(minValue,maxValue);
    }
}
    }
}
