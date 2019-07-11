using System;

namespace ShoutrGui.Dispatcher
{
    public class DispatcherWrapper : IDispatcher
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher;

        public DispatcherWrapper(System.Windows.Threading.Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }
        public void Invoke(Action callback)
        {
            _dispatcher.Invoke(callback);
        }
    }
}