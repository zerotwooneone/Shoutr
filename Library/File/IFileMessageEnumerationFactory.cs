using System;
using System.Collections.Generic;
using Library.Message;

namespace Library.File
{
    public interface IFileMessageEnumerationFactory
    {
        IEnumerable<IBroadcastMessage> Create(string fileName, Guid broadcastId);
    }
}