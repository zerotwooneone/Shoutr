using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shoutr.ByteTransport;
using Shoutr.Contracts;
using Shoutr.Contracts.ByteTransport;
using Shoutr.Contracts.Io;
using Shoutr.Io;
using Unity;

namespace Shoutr.Integration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var tokenSource = new CancellationTokenSource();

            void OnCancelKey(object sender, ConsoleCancelEventArgs e)
            {
                tokenSource.Cancel();
            }

            Console.CancelKeyPress += OnCancelKey;
            var container = new UnityContainer();
            Register(container);
            Func<IUnityContainer> containerFactory = ((IUnityContainer) container).CreateChildContainer;

            try
            {
                PageObservableTest(container, tokenSource.Token).Wait(tokenSource.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var tests = new RegisteredTest[]
            {
                new RegisteredTest
                {
                    Name = nameof(PageObservableTest),
                    Test = u => PageObservableTest(u, tokenSource.Token)
                },
                new RegisteredTest
                {
                    Name = nameof(BroadcastWithoutLossTest),
                    Test = u => BroadcastWithoutLossTest(u, tokenSource.Token)
                }
            };

            var context = new MyContext();
            for (var testIndex = 0; testIndex < tests.Length; testIndex++)
            {
                var test = tests[testIndex];
                Console.WriteLine($"======= Starting test #{testIndex+1} : {test.Name}  ======");
                try
                {
                    Run(containerFactory, test.Test, context, tokenSource.Token).Wait(tokenSource.Token);
                    Console.WriteLine($"== test #{testIndex+1} : {test.Name} Finished");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static async Task Run(Func<IUnityContainer> unityContainer,
            Func<IUnityContainer,Task> test,
            MyContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                await test(unityContainer());
            }
            catch (Exception e)
            {
                context.AddException(e);
                throw;
            }
        }

        private static async Task PageObservableTest(IUnityContainer container, CancellationToken cancellationToken)
        {
            var streamFactory = container.Resolve<IStreamFactory>();
            var reader = streamFactory.CreateReader("test.7z");
            
            const int byteToMegabyteFactor = 1000000;
            var pageSize = 8 * byteToMegabyteFactor;
            var pageArrayObservable = reader
                .PageObservable(pageSize, cancellationToken)
                .ToArray();
            
            var pages = await pageArrayObservable.ToTask(cancellationToken);
            
            var pageCount = pages.Length;
            
            const int expectedPageCount = 1;
            Assert.AreEqual(expectedPageCount, pageCount);

            var lastPage = pages.LastOrDefault();
            const int expectedLastByteCount = 980447;
            Assert.AreEqual(expectedLastByteCount, lastPage.Bytes.Length);
        }

        private static async Task BroadcastWithoutLossTest(IUnityContainer container, CancellationToken cancellationToken)
        {
            var broadcaster = container.Resolve<IBroadcaster>();
            var listener = container.Resolve<IListener>();
            var sender = container.Resolve<IByteSender>();
            var streamFactory = container.Resolve<IStreamFactory>();
            var transporter = new MockByteTransporter(sender);

            transporter.ConfigureSendWithoutLoss();

            var taskFactory = new TaskFactory();
            var listenThread = taskFactory.StartNew(() =>
            {
                try
                {
                    listener.Listen(transporter, streamFactory, cancellationToken).Wait(cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, cancellationToken);
            var sendThread = taskFactory.StartNew(() =>
            {
                try
                {
                    broadcaster.BroadcastFile("test.7z", transporter, streamFactory, cancellationToken: cancellationToken)
                        .Wait(cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, cancellationToken);

            await Task.WhenAll(listenThread, sendThread);
        }

        private static void Register(UnityContainer container)
        {
            container.RegisterType<IListener, Listener>();
            container.RegisterType<IBroadcaster, Broadcaster>();
            container.RegisterFactory<IByteSender>(c =>
            {
                return UdpBroadcastSender.Factory();
            });
            container.RegisterType<IStreamFactory, StreamFactory>();
        }
    }

    internal record RegisteredTest
    {
        public string Name { get; init; }
        public Func<IUnityContainer, Task> Test { get; init; }
    }
}