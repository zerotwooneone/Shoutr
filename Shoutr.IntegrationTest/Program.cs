using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Library.Broadcast;
using Library.File;
using Library.Interface.Broadcast;
using Library.Interface.Configuration;
using Library.Interface.File;
using Library.Interface.Listen;
using Library.Interface.Message;
using Library.Interface.Reactive;
using Library.Reactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using IScheduler = Library.Interface.Reactive.IScheduler;

namespace Shoutr.IntegrationTest
{
    internal class Program
    {
        private readonly IConfiguration _configuration;

        private readonly IListenerFactory _listenerFactory;

        public Program(IListenerFactory listenerFactory,
            IConfiguration configuration)
        {
            _listenerFactory = listenerFactory;
            _configuration = configuration;
        }

        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddCommandLine(args);

            IConfiguration config = builder
                .Build();

            var serviceCollection = new ServiceCollection();

            var startup = new Startup(config);
            startup.ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var listenerFactory = serviceProvider.GetRequiredService<IListenerFactory>();

            var program = new Program(listenerFactory,
                config);

            program.Run(serviceProvider).Wait();
        }

        private async Task Run(IServiceProvider serviceProvider)
        {
            //Console.WriteLine($"Listening on port:{_configuration["listen"]}...");

            //var listener = _listenerFactory.CreateBroadcastListener(int.Parse(_configuration["listen"]));
            //var messages = await listener
            //    .MessagesObservable
            //    .FirstOrDefaultAsync();

            //Console.WriteLine($"Received {JsonConvert.SerializeObject(messages)}");

            var schedulerProvider = new SchedulerProvider();
            var configurationService = new ConfigService();
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
    }

    internal class MockBroadcaster : IBroadcaster, IDisposable
    {
        private readonly string _fileName;

        public MockBroadcaster()
        {
            _fileName = $"glowMaggot.{DateTime.Now:hhmmssffff}.json";
        }

        public async Task Broadcast(IBroadcastHeader header)
        {
            using (var fileStream = GetFileStream())
            {
                await fileStream.WriteLineAsync(JsonConvert.SerializeObject(header));
            }
        }

        public async Task Broadcast(IFileHeader header)
        {
            using (var fileStream = GetFileStream())
            {
                await fileStream.WriteLineAsync(JsonConvert.SerializeObject(header));
            }
        }

        public async Task Broadcast(IChunkHeader header)
        {
            using (var fileStream = GetFileStream())
            {
                await fileStream.WriteLineAsync(JsonConvert.SerializeObject(header));
            }
        }

        public async Task Broadcast(IPayloadMessage header)
        {
            using (var fileStream = GetFileStream())
            {
                await fileStream.WriteLineAsync(JsonConvert.SerializeObject(header));
            }
        }

        public void Dispose()
        {
            
        }

        private StreamWriter GetFileStream()
        {
            return new StreamWriter(new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write,
                FileShare.ReadWrite));
        }
    }

    internal class FileConfig : IFileMessageConfig
    {
        public TimeSpan HeaderRebroadcastInterval => TimeSpan.FromSeconds(5);

        public long MaxPayloadSizeInBytes => 1000;
    }

    internal class SchedulerProvider : ISchedulerProvider
    {
        public readonly SchedulerWrapper SchedulerWrapper = new SchedulerWrapper(Scheduler.Default);
        public IScheduler Default => SchedulerWrapper;
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