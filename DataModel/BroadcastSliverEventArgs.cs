using System;

namespace WpfPractice.DataModel
{
    public class BroadcastSliverEventArgs : EventArgs
    {
        public Guid BroadcastId { get; }
        public uint SliverIndex { get; }

        public BroadcastSliverEventArgs(Guid broadcastId, uint sliverIndex)
        {
            BroadcastId = broadcastId;
            SliverIndex = sliverIndex;
        }
    }
}
