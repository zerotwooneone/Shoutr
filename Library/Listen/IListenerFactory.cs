namespace Library.Listen
{
    public interface IListenerFactory
    {
        IListener CreateBroadcastListener(int port);
    }
}