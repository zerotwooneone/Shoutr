using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shoutr.ByteTransport;
using Shoutr.Contracts;
using Shoutr.Contracts.ByteTransport;
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

            var broadcaster = container.Resolve<IBroadcaster>();
            var listener = container.Resolve<IListener>();
            var sender = container.Resolve<IByteSender>();
            var transporter = new MockByteTransporter(sender);
            
            transporter.ConfigureSendWithoutLoss();

            var taskFactory = new TaskFactory();
            var listenThread = taskFactory.StartNew(() =>
            {
                try
                {
                    listener.Listen(transporter, tokenSource.Token).Wait(tokenSource.Token);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, tokenSource.Token);
            var sendThread = taskFactory.StartNew(() =>
            {
                try
                {
                    broadcaster.BroadcastFile("test.7z", transporter, cancellationToken: tokenSource.Token).Wait(tokenSource.Token);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, tokenSource.Token);

            Task.WaitAll(listenThread, sendThread);
        }

        private static void Register(UnityContainer container)
        {
            container.RegisterType<IListener, Listener>();
            container.RegisterType<IBroadcaster, Broadcaster>();
            container.RegisterFactory<IByteSender>(c =>
            {
                return UdpBroadcastSender.Factory();
            });
        }
    }

    public class MockByteTransporter : IByteReceiver, IByteSender
    {
        private readonly IByteSender _sender;
        public event EventHandler<IBytesReceived> BytesReceived;
        private Action<byte[], Action<byte[]>> onReceive = null;
        private TaskCompletionSource _listTask = new TaskCompletionSource();
        public Task Listen(CancellationToken cancellationToken = default)
        {
            return _listTask.Task;
        }
        
        public void SimulateReceived(byte[] bytes)
        {
            OnBytesReceived(new BytesReceived(bytes));
        }

        public MockByteTransporter(IByteSender sender, int mtu = 1400)
        {
            _sender = sender;
            MaximumTransmittableBytes = mtu;
        }

        public void ConfigureSend(Action<byte[], Action<byte[]>> handleReceive)
        {
            onReceive = handleReceive;
        }
        
        public void ConfigureSendWithoutLoss()
        {
            ConfigureSend((b, a) => a(b));
        }

        public int MaximumTransmittableBytes { get; }

        public async Task Send(byte[] bytes)
        {
            //DdsLog($"about to send {bytes.Length}", true);
            await _sender.Send(bytes);
            //DdsLog($"about to receive {bytes.Length}", true);
            onReceive?.Invoke(bytes,SimulateReceived);
            DdsLog($"received {bytes.Length}", true);
        }
        
        internal static void DdsLog(string message, 
            bool includeDetails = false,
            [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            if (includeDetails)
            {
                Console.Write($"{caller} thread:{System.Threading.Thread.CurrentThread.ManagedThreadId} ");    
            }
            Console.WriteLine($"{message}");
        }

        protected virtual void OnBytesReceived(IBytesReceived e)
        {
            BytesReceived?.Invoke(this, e);
        }
    }

    public class BytesReceived : IBytesReceived
    {
        public byte[] Bytes { get; }

        public BytesReceived(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}