namespace Library.Interface.Listen
{
    public interface IListenerFactory
    {
        IListener CreateBroadcastListener(int port);
    }
}