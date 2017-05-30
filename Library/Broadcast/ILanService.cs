using System.Threading.Tasks;

namespace Library.Broadcast
{
    /// <summary>
    /// Provides logic around the boradcasting and listening for data on the LAN
    /// </summary>
    public interface ILanService
    {
        Task Broadcast(byte[] bytes);
    }
}