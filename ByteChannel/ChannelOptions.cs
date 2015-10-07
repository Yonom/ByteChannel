using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteChannel
{
    class ChannelOptions
    {
        public int MaximumConcurrency { get; }
        public TimeSpan WaitTimeout { get; }

        public ChannelOptions(int maximumConcurrency, TimeSpan waitTimeout)
        {
            this.MaximumConcurrency = maximumConcurrency;
            this.WaitTimeout = waitTimeout;
        }
    }
}
