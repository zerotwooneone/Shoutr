using Library.Message;

namespace Library.File
{
    public interface IBroadcastMessageConversionService
    {
        byte[] Convert(BroadcastMessage broadcastMessage);
        BroadcastMessage Convert(byte[] bytes);
    }
}