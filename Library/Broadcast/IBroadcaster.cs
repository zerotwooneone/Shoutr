using Library.Message;
using System.Threading.Tasks;

namespace Library.Broadcast
{
    public interface IBroadcaster
    {
        Task Broadcast(IBroadcastHeader broadcastHeader);
        Task Broadcast(IFileHeader fileHeader);
        Task Broadcast(IChunkHeader chunkHeader);
        Task Broadcast(IPayloadMessage payloadMessage);
    }
}