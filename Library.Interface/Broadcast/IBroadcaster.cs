using System.Threading.Tasks;
using Library.Interface.Message;

namespace Library.Interface.Broadcast
{
    public interface IBroadcaster
    {
        Task Broadcast(IBroadcastHeader broadcastHeader);
        Task Broadcast(IFileHeader fileHeader);
        Task Broadcast(IChunkHeader chunkHeader);
        Task Broadcast(IPayloadMessage payloadMessage);
    }
}