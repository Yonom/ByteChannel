using System;

namespace ByteChannel
{
    public class ChannelOptions
    {
        public int MaximumConcurrency { get; }
        public TimeSpan WaitTimeout { get; }

        public ChannelOptions(int maximumConcurrency, TimeSpan waitTimeout)
        {
            if (maximumConcurrency > Config.MaxQueueSize)
                throw new ArgumentException("Maximum concurrency can not be more than " + Config.MaxQueueSize + ".",
                    nameof(maximumConcurrency));

            this.MaximumConcurrency = maximumConcurrency;
            this.WaitTimeout = waitTimeout;
        }

        public static ChannelOptions Default = new ChannelOptions(Config.MaxQueueSize, TimeSpan.FromMilliseconds(500));
    }
}
