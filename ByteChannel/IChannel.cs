namespace ByteChannel
{
    /// <summary>
    /// Represents a byte array channel.
    /// </summary>
    /// <typeparam name="T">The type of receive messages.</typeparam>
    public interface IChannel<out T>
    {
        /// <summary>
        /// Sends the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        void Send(byte[] data);
        /// <summary>
        /// Occurs when data is received.
        /// </summary>
        event ReceiveCallback<T> Receive;
    }
}