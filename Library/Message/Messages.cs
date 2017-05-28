namespace Library.Message
{
    public class Messages
    {
        public IBroadcastHeader BroadcastHeader { get; set; }
        public IPayloadMessage PayloadMessage { get; set; }
        public IChunkHeader ChunkHeader { get; set; }
        public IFileHeader FileHeader { get; set; }
    }
}