using System;
using System.Collections.Generic;

namespace ShoutrGui.DataModel
{
    public class BroadcastViewmodelParams
    {
        public Guid BroadcastId { get; set; }
        public IEnumerable<SliverViewmodelParams> Slivers { get; set; }
    }
}
