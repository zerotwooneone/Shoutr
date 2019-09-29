namespace Library.Message
{
    public interface IReceivedMessage : IMessages
    {
        string SenderId { get; }
    }
}