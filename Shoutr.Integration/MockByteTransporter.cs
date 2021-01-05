using System;
using System.Threading;
using System.Threading.Tasks;
using Shoutr.Contracts.ByteTransport;

namespace Shoutr.Integration
{
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
            //DdsLog($"received {bytes.Length}", true);
        }
        
        internal static void DdsLog(string message, 
            bool includeDetails = false,
            [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            if (includeDetails)
            {
                Console.WriteLine($"{caller} thread:{System.Threading.Thread.CurrentThread.ManagedThreadId} ");    
            }
            Console.WriteLine($"{message}");
        }

        protected virtual void OnBytesReceived(IBytesReceived e)
        {
            BytesReceived?.Invoke(this, e);
        }

        public void StopListening()
        {
            _listTask.SetResult();
        }
    }
}