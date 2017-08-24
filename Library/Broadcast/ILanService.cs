using System.Threading.Tasks;

namespace Library.Broadcast
{
    /// <summary>
    /// Provides logic around the broadcasting and listening for data on the LAN
    /// </summary>
    public interface ILanService
    {
        Task Broadcast(byte[] bytes);
        bool DequeueInProgress { get; }
    }
}