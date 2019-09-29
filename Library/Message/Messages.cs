namespace Library.Message
{
    /// <summary>
    /// A wrapper which is returned when deserializing bytes into messages. 
    /// </summary>
    public class Messages : IMessages
    {
        public IBroadcastHeader BroadcastHeader { get; set; }
        public IPayloadMessage PayloadMessage { get; set; }
        public IChunkHeader ChunkHeader { get; set; }
        public IFileHeader FileHeader { get; set; }
    }
}