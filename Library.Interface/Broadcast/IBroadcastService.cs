using System.Threading.Tasks;
using Library.Interface.File;
using Library.Interface.Reactive;

namespace Library.Interface.Broadcast
{
    public interface IBroadcastService
    {
        /// <summary>
        /// Broadcasts a single file.
        /// </summary>
        /// <param name="fileName">the path to the file</param>
        /// <param name="fileMessageConfig">config for reading files</param>
        /// <param name="broadcaster">the util used to actually broadcast</param>
        /// <param name="scheduler">A scheduler used for throttling and controlling async work</param>
        /// <returns>A task representing the work of sending out all of the messages for the entire broadcast</returns>
        Task BroadcastFile(string fileName, 
            IFileMessageConfig fileMessageConfig,
            IBroadcaster broadcaster,
            IScheduler scheduler);
    }
}