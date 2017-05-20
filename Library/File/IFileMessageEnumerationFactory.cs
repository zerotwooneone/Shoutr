using System;
using System.Collections.Generic;
using Library.Message;

namespace Library.File
{
    public interface IFileMessageEnumerationFactory
    {
        IEnumerable<BroadcastMessage> Create(string fileName, Guid broadcastId);
    }
}