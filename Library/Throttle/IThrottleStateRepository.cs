using System;
using System.Collections.Generic;

namespace Library.Throttle
{
    public interface IThrottleStateRepository
    {
        DateTime GetDateTime();
        void AddRecord(DateTime dateTime);
        IEnumerable<DateTime> GetRecords();
        void DeleteRecords(IEnumerable<DateTime> dateTimes);
    }
}