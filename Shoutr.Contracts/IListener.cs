using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shoutr.Contracts
{
    public interface IListener
    {
        Task Listen(int port = Defaults.Port,
            CancellationToken cancellationToken = default);

        event EventHandler<IBroadcastResult> BroadcastEnded;
    }
}