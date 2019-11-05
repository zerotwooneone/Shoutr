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
            var fdr = new Library.File.NaiveFileDataRepository();
            //var r = await fdr.SetPage("test123", 0, new byte[]{1,2,3,4,5,6,7,8,9,0});
            //if (!r.Success)
            //{
            //    Console.WriteLine(r.Exception);
            //}

            const string fileName = "glowMaggot.png";

            var byteCount = fdr.GetByteCount(fileName);

            var tasks = new List<Task<Library.File.FileWriteResult>>();
            for (int taskIndex = 0; taskIndex<20000; taskIndex++)
            {
                var pageSize = (uint)RandomGen3.Next(1, 400);
                    var maxPageCount = (int)(byteCount / pageSize);
                    int pageIndex = RandomGen3.Next(0, maxPageCount);
                var t = fdr.GetPage(fileName, (int)pageSize, pageIndex)
                .ContinueWith(t1=>{
                    var bytes = t1.Result.Bytes;
                    var startIndex = pageIndex * pageSize;                    
                    return fdr.SetPage("glowMaggot-Copy.png", pageSize * pageIndex, bytes);
                }).Unwrap();
                                
                tasks.Add(t);
            }

            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                var result = await task;
                if (!result.Success)
                {
                    Console.WriteLine(result.Exception);
                }
            }
            
            //Console.WriteLine($"Listening on port:{_configuration["listen"]}...");

            //var listener = _listenerFactory.CreateBroadcastListener(int.Parse(_configuration["listen"]));
            //var messages = await listener
            //    .MessagesObservable
            //    .FirstOrDefaultAsync();
            
            //Console.WriteLine($"Received {JsonConvert.SerializeObject(messages)}");

            
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
