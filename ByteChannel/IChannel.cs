namespace ByteChannel
{
    /// <summary>
    /// Represents a byte array channel.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    public interface IChannel<in TInput, out TOutput>
    {
        /// <summary>
        /// Sends the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        void Send(TInput data);
        /// <summary>
        /// Occurs when data is received.
        /// </summary>
        event ChannelCallback<TOutput> Receive;
    }
}