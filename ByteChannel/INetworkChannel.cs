namespace ByteChannel
{
    /// <summary>
    /// Represents a channel which connects ByteChannel to an external source.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    public interface INetworkChannel<TSender> : IChannel<byte[], Message<TSender>>
    {
        /// <summary>
        /// Gets the size of bytes this channel requires.
        /// </summary>
        /// <value>
        /// The size of bytes this channel requires.
        /// </value>
        int Size { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is still busy sending messages.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is busy; otherwise, <c>false</c>.
        /// </value>
        bool IsBusy { get; }
    }
}