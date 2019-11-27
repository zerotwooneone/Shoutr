using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Library.Broadcast;
using Library.Configuration;
using Library.File;
using Library.Listen;
using Library.Message;
using Library.Reactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Shoutr.IntegrationTest
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
            //Console.WriteLine($"Listening on port:{_configuration["listen"]}...");

            //var listener = _listenerFactory.CreateBroadcastListener(int.Parse(_configuration["listen"]));
            //var messages = await listener
            //    .MessagesObservable
            //    .FirstOrDefaultAsync();

            //Console.WriteLine($"Received {JsonConvert.SerializeObject(messages)}");

            var schedulerProvider = new SchedulerProvider();
            ConfigService configurationService = new ConfigService();
            var m = new NaiveBroadcastService(
                new MessageObservableFactory(
                    new FileMessageService(
                        new NaiveFileDataRepository(),
                        configurationService)),
                configurationService);

            using (var broadcaster = new MockBroadcaster())
            {
                await m.BroadcastFile("glowMaggot.png", new FileConfig(), broadcaster, schedulerProvider.Default);
            }
        }

        private readonly IListenerFactory _listenerFactory;
        private readonly IConfiguration _configuration;

        public Program(IListenerFactory listenerFactory,
            IConfiguration configuration)
        {
            _listenerFactory = listenerFactory;
            _configuration = configuration;
        }
    }

    internal class MockBroadcaster : IBroadcaster, IDisposable
    {
        private StreamWriter _fileStream;

        public MockBroadcaster()
        {
            _fileStream = new StreamWriter(new FileStream($"glowMaggot.{DateTime.Now.ToString("hhmmssffff")}.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite));
        }

        public async Task Broadcast(IBroadcastHeader header)
        {
            await _fileStream.WriteLineAsync(JsonConvert.SerializeObject(header));
        }

        public async Task Broadcast(IFileHeader header)
        {
            await _fileStream.WriteLineAsync(JsonConvert.SerializeObject(header));
        }

        public async Task Broadcast(IChunkHeader header)
        {
            await _fileStream.WriteLineAsync(JsonConvert.SerializeObject(header));
        }

        public async Task Broadcast(IPayloadMessage header)
        {
            await _fileStream.WriteLineAsync(JsonConvert.SerializeObject(header));
        }

        public void Dispose()
        {
            _fileStream.Flush();
            _fileStream.Dispose();
            _fileStream = null;
        }
    }

    internal class FileConfig : IFileMessageConfig
    {
        public TimeSpan HeaderRebroadcastInterval => TimeSpan.FromSeconds(5);

        public long MaxPayloadSizeInBytes => 1000;
    }

    internal class SchedulerProvider : ISchedulerProvider
    {
        public IScheduler Default => Scheduler.Default;
    }

    internal class ConfigService : IConfigurationService
    {
        public int BroadcastPort => throw new NotImplementedException();

        public int? MaxBroadcastsPerSecond => throw new NotImplementedException();

        public uint PageSize => 2000;

        public TimeSpan? TimeBetweenBroadcasts => TimeSpan.FromMilliseconds(1);

        public int PayloadSizeInBytes => throw new NotImplementedException();
    }
}
