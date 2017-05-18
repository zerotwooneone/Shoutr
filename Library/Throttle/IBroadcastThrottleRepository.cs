using System;

namespace Library.Throttle
{
    public interface IBroadcastThrottleRepository
    {
        BroadcastAttempt SaveAttempt(int port, DateTime timeStamp);
    }
}