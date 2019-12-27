namespace Library.Interface.Message
{
    /// <summary>
    /// Represents the meta data general to a whole file
    /// </summary>
    public interface IFileHeader : IBroadcastMessage, IMessageHeader
    {
        string FileName { get; }
        long FirstPayloadIndex { get; }
    }
}