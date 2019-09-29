namespace Library.Message
{
    public interface IMessages
    {
        IBroadcastHeader BroadcastHeader { get; }
        IPayloadMessage PayloadMessage { get; }
        IChunkHeader ChunkHeader { get; }
        IFileHeader FileHeader { get; }
    }
}