using Library.Message;
using System;
using System.Reactive.Concurrency;

namespace Library.File
{
    public interface IBroadcastMessageObservableFactory
    {
        /// <summary>
        /// Returns an observable which represents all of the messages of a broadcast which contains the specified file
        /// </summary>
        /// <param name="fileName">path to the file</param>
        /// <param name="fileMessageConfig">config for reading a file</param>
        /// <param name="broadcastId">optional broadcast id</param>
        /// <param name="scheduler">optional scheduler</param>
        /// <returns></returns>
        IObservable<IMessages> GetFileBroadcast(string fileName, 
            IFileMessageConfig fileMessageConfig,
            Guid? broadcastId = null,
            IScheduler scheduler = null);
    }
}