using System;
using Library.Interface.Message;
using Library.Interface.Reactive;

namespace Library.Interface.File
{
    public interface IBroadcastMessageObservableFactory
    {
        /// <summary>
        /// Returns an observable which represents all of the messages of a broadcast which contains the specified file
        /// </summary>
        /// <param name="fileName">path to the file</param>
        /// <param name="fileMessageConfig">config for reading a file</param>
        /// <param name="scheduler">the scheduler for timing messages</param>
        /// <param name="broadcastId">optional broadcast id</param>        
        /// <returns></returns>
        IObservable<IMessages> GetFileBroadcast(string fileName, 
            IFileMessageConfig fileMessageConfig,
            IScheduler scheduler,
            Guid? broadcastId = null);
    }
}