using System;
using Library.Interface.Message;

namespace Library.Interface.Listen
{
    public interface IListener
    {
        IObservable<IReceivedMessage> MessagesObservable { get; }
    }
}