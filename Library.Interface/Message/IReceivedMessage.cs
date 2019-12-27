namespace Library.Interface.Message
{
    public interface IReceivedMessage : IMessages
    {
        string SenderId { get; }
    }
}