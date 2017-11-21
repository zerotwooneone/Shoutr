using System;

namespace WpfPractice.DataModel
{
    public class SliverViewmodelParams
    {
        public Guid BroadcastId { get; set; }
        public uint SliverIndex { get; set; }
        public bool? Success { get; set; }
    }
}