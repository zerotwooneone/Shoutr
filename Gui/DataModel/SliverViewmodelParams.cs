using System;
using ShoutrGui.Dispatcher;

namespace ShoutrGui.DataModel
{
    public class SliverViewmodelParams
    {
        public SliverViewmodelParams(IDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public Guid BroadcastId { get; set; }
        public uint SliverIndex { get; set; }
        public bool? Success { get; set; }
        public IDispatcher Dispatcher { get; }
    }
}