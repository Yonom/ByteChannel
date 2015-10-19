using System;
using System.Collections.Generic;

namespace ByteChannel
{
    /// <summary>
    /// Represents a channel through which bytes can be sent.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    public class ByteChannel<TSender> : IMessageChannel<TSender>, IDisposable
    {
        private readonly Dictionary<TSender, MessageBuffer> _buffers = new Dictionary<TSender, MessageBuffer>();
        private readonly PacketBuffer<TSender> _buffer;
        private bool _newUsers = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteChannel{TSender}"/> class.
        /// </summary>
        /// <param name="innerChannel">The inner network channel.</param>
        public ByteChannel(INetworkChannel<TSender> innerChannel) 
            : this(innerChannel, ByteChannelOptions.Default)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteChannel{TSender}" /> class.
        /// </summary>
        /// <param name="innerChannel">The inner network channel.</param>
        /// <param name="options">The options.</param>
        public ByteChannel(INetworkChannel<TSender> innerChannel, ByteChannelOptions options)
        {
            this._buffer =
                new PacketBuffer<TSender>(
                    new PacketChecker<TSender>(
                        new PacketPadder<TSender>(innerChannel), options));

            this._buffer.Receive += this._buffer_Receive;

            this.Reset();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            this._buffer.Dispose();
            this._buffer.Receive -= this._buffer_Receive;
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event ChannelCallback<Message<TSender>> Receive;


        /// <summary>
        /// Occurs when a sender is removed.
        /// </summary>
        public event ChannelCallback<TSender> RemovedSender;

        /// <summary>
        /// Occurs when a sender is discovered.
        /// </summary>
        public event ChannelCallback<TSender> DiscoverSender;

        /// <summary>
        /// Sends the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(byte[] data)
        {
            if (this._newUsers)
                lock (this._buffers)
                    if (this._newUsers)
                        this.Reset();

            this._buffer.Send(
                ByteHelper.Combine(
                    ByteHelper.EncodeVarint(data.Length),
                    data));
        }

        /// <summary>
        /// Queues a reset message. This function is automatically called whenever a new user is detected.
        /// </summary>
        public void Reset()
        {
            this._newUsers = false;
            this._buffer.Send(new byte[0]);
        }

        /// <summary>
        /// Removes the given sender's buffer. Use when a user logs off to free memory.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveSender(TSender sender)
        {
            lock (this._buffers) this._buffers.Remove(sender);
            this._buffer.RemoveSender(sender);

            this.RemovedSender?.Invoke(this, sender);
        }

        private void _buffer_Receive(object sender, SegmentMessage<TSender> e)
        {
            bool newUser = false;
            MessageBuffer buff;
            lock (this._buffers)
            {
                if (!this._buffers.TryGetValue(e.Sender, out buff))
                {
                    if (e.Data.Count > 0) return;
                    newUser = true;
                    if (!e.IsOwnMessage) this._newUsers = true;
                    this._buffers.Add(e.Sender, buff = new MessageBuffer());
                }
            }

            if (newUser) this.DiscoverSender?.Invoke(this, e.Sender);

            lock (buff)
            {
                if (e.Data.Count == 0)
                {
                    buff.Reset();
                    return;
                }

                for (var i = e.Data.Offset; i < e.Data.Offset + e.Data.Count; i++)
                {
                    buff.AddByte(e.Data.Array[i]);

                    var bytes = buff.CheckForMessages();
                    if (bytes != null)
                    {
                        this.Receive?.Invoke(this, new Message<TSender>(e.Sender, e.IsOwnMessage, bytes));
                    }
                }
            }
        }

        private class MessageBuffer
        {
            private List<byte> _receivedBytes;
            private int _length;
            private ParseState _state;
            private int _bytePlace;

            public void AddByte(byte b)
            {
                switch (this._state)
                {
                    case ParseState.Header:
                        this._length |= (b & 0x7f) << (7 * this._bytePlace++);
                        if ((b & 0x80) != 0x80)
                        {
                            this._state = ParseState.Body;
                            this._receivedBytes = new List<byte>(Math.Max(0, Math.Min(this._length, ushort.MaxValue)));
                        }
                        break;
                    case ParseState.Body:
                        this._receivedBytes.Add(b);
                        break;
                }
            }

            public byte[] CheckForMessages()
            {
                if (this._length != this._receivedBytes.Count)
                    return null;

                try
                {
                    return this._receivedBytes.ToArray();
                }
                finally
                {
                    this.Reset();
                }
            }

            public void Reset()
            {
                this._bytePlace = 0;
                this._length = 0;
                this._receivedBytes = null;
                this._state = ParseState.Header;
            }
        }

        private enum ParseState
        {
            Header,
            Body
        }
    }
}