using System;
using System.IO;
using Microsoft.Extensions.Configuration;

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

            var testSection = config.GetSection("test");
            var testConfig = testSection.Get<TestConfig>();

            Console.WriteLine($" Hello { testConfig.Name } and {config["listen"]} !");
        }
    }
}
