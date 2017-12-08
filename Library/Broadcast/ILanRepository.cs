using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Library.Broadcast
{
    /// <summary>
    /// Represents access to read and write local area network data
    /// </summary>
    public interface ILanRepository
    {
        Task Broadcast(byte[] data);
        EventHandler<UdpReceiveResult> OnReceived();
        void AddToQueue(byte[] broadcast);
        byte[] PopQueue();
        bool QueueIsEmpty { get; }
        /// <summary>
        /// Represents the loop which dequeues continuously. This should be initialized to Task.CompletedTask and therefor never be null.
        /// </summary>
        Task DequeueTask { get; set; }
    }
}
