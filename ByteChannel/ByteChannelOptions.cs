using System;

namespace ByteChannel
{
    /// <summary>
    /// Holds the configuration for a ByteChannel
    /// </summary>
    public class ByteChannelOptions
    {
        /// <summary>
        /// Gets the maximum number of packets that will be simultaneously sent over the network.
        /// </summary>
        /// <value>
        /// The maximum concurrency.
        /// </value>
        public int MaximumConcurrency { get; }

        /// <summary>
        /// Gets the timeout before ByteChannel considers all travelling packets as failed.
        /// </summary>
        /// <value>
        /// The wait timeout.
        /// </value>
        public TimeSpan WaitTimeout { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteChannelOptions"/> class.
        /// </summary>
        /// <param name="maximumConcurrency">The maximum number of packets that will be simultaneously sent over the network.</param>
        /// <param name="waitTimeout">The timeout before ByteChannel considers all travelling packets as failed.</param>
        public ByteChannelOptions(int maximumConcurrency, TimeSpan waitTimeout)
        {
            if (maximumConcurrency > Config.MaxQueueSize)
                throw new ArgumentException("Maximum concurrency can not be more than " + Config.MaxQueueSize + ".",
                    nameof(maximumConcurrency));

            this.MaximumConcurrency = maximumConcurrency;
            this.WaitTimeout = waitTimeout;
        }

        /// <summary>
        /// The default ByteChannel configuration
        /// </summary>
        public static readonly ByteChannelOptions Default = 
            new ByteChannelOptions(Config.MaxQueueSize, TimeSpan.FromMilliseconds(Config.DefaultWaitTimeout));
    }
}
