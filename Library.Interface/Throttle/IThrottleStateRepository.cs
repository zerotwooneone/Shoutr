using System;
using System.Collections.Generic;

namespace Library.Interface.Throttle
{
    /// <summary>
    /// Represents the data we track related to throttling
    /// </summary>
    public interface IThrottleStateRepository
    {
        DateTime GetDateTime();
        void AddRecord(DateTime dateTime);
        IEnumerable<DateTime> GetRecords();
        void DeleteRecords(IEnumerable<DateTime> dateTimes);
    }
}