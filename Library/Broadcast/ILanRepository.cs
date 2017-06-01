﻿using System;
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
        [Obsolete("Use PopQueue instead")]
        IEnumerable<byte[]> GetQueue();
        void AddToQueue(byte[] data);
        byte[] PopQueue();
        bool QueueIsEmpty { get; }
    }
}
