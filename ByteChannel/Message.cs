using System;

namespace ByteChannel
{
    internal class SegmentMessage<TSender> : Message<TSender, ArraySegment<byte>>
    {
        public SegmentMessage(TSender sender, bool isOwnMessage, ArraySegment<byte> data) : base(sender, isOwnMessage, data)
        {
        }
    }

    /// <summary>
    /// Represents a received message.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    public class Message<TSender> : Message<TSender, byte[]>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message{TSender}"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="isOwnMessage">if set to <c>true</c> marks that this message is an echo of a sent message. (Required for error checking)</param>
        /// <param name="data">The data.</param>
        public Message(TSender sender, bool isOwnMessage, byte[] data) : base(sender, isOwnMessage, data)
        {
        }
    }
    /// <summary>
    /// Represents a received message.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class Message<TSender, TData> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message{TSender, TData}"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="isOwnMessage">if set to <c>true</c> marks that this message is an echo of a sent message. (Required for error checking)</param>
        /// <param name="data">The data.</param>
        public Message(TSender sender, bool isOwnMessage, TData data)
        {
            this.Sender = sender;
            this.IsOwnMessage = isOwnMessage;
            this.Data = data;
        }

        /// <summary>
        /// Gets a value indicating whether this is an echoed message from the server.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is an echoed message; otherwise, <c>false</c>.
        /// </value>
        public bool IsOwnMessage { get; }
        /// <summary>
        /// Gets the sender.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        public TSender Sender { get; }
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public TData Data { get; }
    }
}