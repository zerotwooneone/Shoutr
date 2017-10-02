﻿using System;
using System.Collections.Concurrent;

namespace WpfPractice.Listen
{
    public class ListenService : IListenService
    {
        private readonly ConcurrentQueue<Guid> _broadcastIds;

        public ListenService()
        {
            _broadcastIds = new ConcurrentQueue<Guid>();

            Extensions.Repeat(() =>
            {
                _broadcastIds.Enqueue(Guid.NewGuid());
                OnNewBroadcast();
            }, 1000, .7, 4);
        }

        public Guid GetNextBroadcastId()
        {
            return Guid.NewGuid();
        }

        public event EventHandler NewBroadcast;

        protected virtual void OnNewBroadcast()
        {
            NewBroadcast?.Invoke(this, EventArgs.Empty);
        }
    }
}