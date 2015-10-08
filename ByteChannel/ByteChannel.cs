using System;
using System.Collections.Generic;
using System.Linq;

namespace ByteChannel
{
    /// <summary>
    /// Represents a channel through which bytes can be sent.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    public class ByteChannel<TSender> : IChannel<byte[], Message<TSender>>, IDisposable
    {
        private readonly Dictionary<TSender, MessageBuffer> _buffers = new Dictionary<TSender, MessageBuffer>();
        private readonly PacketBuffer<TSender> _buffer;
        private bool _newUsers = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteChannel{TSender}"/> class.
        /// </summary>
        /// <param name="packet">The data packet channel.</param>
        public ByteChannel(IDataPacket<TSender> packet) 
            : this(packet, ChannelOptions.Default)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteChannel{TSender}" /> class.
        /// </summary>
        /// <param name="packet">The data packet channel.</param>
        /// <param name="options">The options.</param>
        public ByteChannel(IDataPacket<TSender> packet, ChannelOptions options)
        {
            this._buffer =
                new PacketBuffer<TSender>(
                    new PacketChecker<TSender>(
                        new PacketPadder<TSender>(packet), options));

            this._buffer.Receive += this._buffer_Receive;

            this.Reset();
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event ReceiveCallback<Message<TSender>> Receive;

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
        /// Removes the given sender's buffer. Use when a user logs off.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveSender(TSender sender)
        {
            throw new NotImplementedException();
        }

        private void _buffer_Receive(object sender, SegmentMessage<TSender> e)
        {
            MessageBuffer buff;
            lock (this._buffers)
            {
                if (!this._buffers.TryGetValue(e.Sender, out buff))
                {
                    if (e.Data.Count > 0) return;
                    this._newUsers = true;
                    this._buffers.Add(e.Sender, buff = new MessageBuffer());
                }
            }

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
                            this._receivedBytes = new List<byte>(Math.Max(this._length, ushort.MaxValue));
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
        
        public void Dispose()
        {
            this._buffer.Dispose();
        }
    }
}