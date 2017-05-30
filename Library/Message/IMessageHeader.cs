namespace Library.Message
{
    /// <summary>
    /// Represents the meta data shared between all header messages
    /// </summary>
    public interface IMessageHeader
    {
        bool? IsLast { get; }
    }
}