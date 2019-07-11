using System;

namespace ShoutrGui.Dispatcher
{
    public interface IDispatcher
    {
        void Invoke(Action callback);
    }
}