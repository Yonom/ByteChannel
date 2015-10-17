namespace ByteChannel
{
    /// <summary>
    /// Represents a channel through which bytes can be sent.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    public interface IByteChannel<TSender> : IChannel<byte[], Message<TSender>>
    {
        /// <summary>
        /// Queues a reset message. This function is automatically called whenever a new user is detected.
        /// </summary>
        void Reset();
        /// <summary>
        /// Removes the given sender's buffer. Use when a user logs off to free memory.
        /// </summary>
        /// <param name="sender">The sender.</param>
        void RemoveSender(TSender sender);

        /// <summary>
        /// Occurs when a sender is discovered.
        /// </summary>
        event ChannelCallback<TSender> DiscoverSender;

        /// <summary>
        /// Occurs when a sender is removed.
        /// </summary>
        event ChannelCallback<TSender> RemovedSender;
    }
}