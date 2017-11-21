using System;
using System.Collections.Generic;

namespace WpfPractice.DataModel
{
    public class BroadcastViewmodelParams
    {
        public Guid BroadcastId { get; set; }
        public IEnumerable<SliverViewmodelParams> Slivers { get; set; }
    }
}
