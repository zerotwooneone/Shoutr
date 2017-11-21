using System;

namespace ShoutrGui.DataModel
{
    public class SliverChangedParams
    {
        public Guid BroadcastId { get; set; }
        public uint SliverIndex { get; set; }
        public bool Success { get; set; }
    }
}