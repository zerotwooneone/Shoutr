using System;
using System.Collections.Generic;

namespace Shoutr.Integration
{
    internal class MyContext
    {
        private readonly IList<Exception> _exceptions = new List<Exception>();

        public void AddException(Exception exception)
        {
            _exceptions.Add(exception);
        }

        public IList<Exception> Exceptions => _exceptions;
    }
}