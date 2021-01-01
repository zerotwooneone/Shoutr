using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using ProtoBuf;
using Shoutr.Contracts;

namespace Shoutr.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");

            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var tokenSource = new CancellationTokenSource();

            void OnCancelKey(object sender, ConsoleCancelEventArgs e)
            {
                tokenSource.Cancel();
            }

            System.Console.CancelKeyPress += OnCancelKey;
            
            try
            {
                if (config["listen"] != null)
                {
                    System.Console.WriteLine("Going to listen");
                    IListener l = new Listener();
                    l.BroadcastEnded += (s, r) =>
                    {
                        System.Console.WriteLine($"bcid:{r.BroadcastId} file:{r.FileName}");
                    };
                    l.ListenUdpBroadcast(cancellationToken: tokenSource.Token).Wait(tokenSource.Token);
                }
                else if (config["broadcast"] != null)
                {
                    const string fileName = "test.7z";

                    System.Console.WriteLine($"Going to broadcast {fileName}");
                    var b = new Broadcaster();
                    b.BroadcastFileUdp(fileName, cancellationToken: tokenSource.Token).Wait(tokenSource.Token);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"fatal error: {e}");
            }
        }
    }
}
