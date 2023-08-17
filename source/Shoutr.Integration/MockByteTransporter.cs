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
            await _sender.Send(bytes);
            onReceive?.Invoke(bytes,SimulateReceived);
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