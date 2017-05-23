using System.IO;
using Library.File;
using ProtoBuf;

namespace Library.Message
{
    public class BroadcastMessageConversionService : IBroadcastMessageConversionService
    {
        public byte[] Convert(BroadcastMessage broadcastMessage)
        {
            using (var stream =new MemoryStream())
            {
                Serializer.Serialize(stream,broadcastMessage);
                return stream.ToArray();
            }
        }

        public BroadcastMessage Convert(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                 return Serializer.Deserialize<BroadcastMessage>(stream);
            }
        }
    }
}