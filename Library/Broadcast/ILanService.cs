using System.Threading.Tasks;

namespace Library.Broadcast
{
    /// <summary>
    /// Provides logic around the broadcasting and listening for data on the LAN
    /// </summary>
    public interface ILanService
    {
        /// <summary>
        /// Adds bytes to the queue of bytes to be broadcast. Call StartBroadcast to begin transmitting
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        void Broadcast(byte[] bytes);

        /// <summary>
        /// True when the broadcast queue is being drained. False when the queue is empty or broadcasting has been paused.
        /// </summary>
        bool DequeueInProgress { get; }

        /// <summary>
        /// Starts a task which will continuously dequeue from the repository. The task ends when the queue is empty or the throttle service becomes paused
        /// </summary>
        /// <returns>The new task that was created</returns>
        /// <exception cref="">Throws an exception is the task is already in progress</exception>
        Task StartBroadcast();
    }
}