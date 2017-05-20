﻿using System.ComponentModel;
using Library.Broadcast;
using Library.Configuration;

namespace Library.Throttle
{
    public class BroadcastThrottleService : IBroadcastThrottleService
    {
        private readonly IThrottleStateFactory _throttleStateFactory;
        private readonly IConfigurationService _configurationService;

        public BroadcastThrottleService(IThrottleStateFactory throttleStateFactory,
            IConfigurationService configurationService)
        {
            _throttleStateFactory = throttleStateFactory;
            _configurationService = configurationService;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void AddCount()
        {
            throw new System.NotImplementedException();
        }

        public bool Paused { get; }
    }
}