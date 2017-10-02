using System;
using WpfPractice.Listen;

namespace WpfPractice.Mock
{
    public class MockListenBroadcastViewmodel : IBroadcastViewmodel
    {
        public MockListenBroadcastViewmodel()
        {
            BroadcastId = Guid.NewGuid();
        }

        public Guid BroadcastId { get; }
    }
}