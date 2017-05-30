namespace Library.Message
{
    /// <summary>
    /// Converts between messages and the bytes that will be transmitted
    /// </summary>
    public interface IBroadcastMessageConversionService
    {
        byte[] Convert(IBroadcastHeader broadcastHeader);
        byte[] Convert(IFileHeader fileHeader);
        byte[] Convert(IChunkHeader chunkHeader);
        byte[] Convert(IPayloadMessage payloadMessage);
        Messages Convert(byte[] bytes);
    }
}