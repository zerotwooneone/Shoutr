namespace Library.Message
{
    /// <summary>
    /// Represents meta data general enough for the whole broadcast
    /// </summary>
    public interface IBroadcastHeader : IBroadcastMessage, IMessageHeader
    {
        long MaxPayloadSizeInBytes { get; }
    }
}