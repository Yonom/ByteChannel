using System;
using System.Collections.Generic;
using System.Linq;

namespace ByteChannel
{
    public class ByteChannel<T> : IChannel<Message<T>>
    {
        private readonly Dictionary<T, MessageBuffer> _buffers = new Dictionary<T, MessageBuffer>();
        private readonly IChannel<Message<T>> _channel;
        private bool _newUsers = true;

        public ByteChannel(IDataPacket<T> packet)
        {
            this._channel =
                new PacketBuffer<T>(
                    new PacketChecker<T>(
                        new PacketPadder<T>(
                            packet)));

            this._channel.Receive += this._channel_Receive;

            this.Reset();
        }

        public event ReceiveCallback<Message<T>> Receive;

        public void Send(byte[] data)
        {
            if (this._newUsers)
                lock (this._buffers)
                    if (this._newUsers)
                        this.Reset();
                    
            this._channel.Send(
                ByteHelper.Combine(
                    ByteHelper.EncodeVarint(data.Length), 
                    data));
        }

        public void Reset()
        {
            this._newUsers = false;
            this._channel.Send(new byte[0]);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void RemoveSender(T sender)
        {
            throw new NotImplementedException();
        }

        private void _channel_Receive(object sender, Message<T> e)
        {
            MessageBuffer buff;
            lock (this._buffers)
            {
                if (!this._buffers.TryGetValue(e.Sender, out buff))
                {
                    if (e.Data.Any()) return;
                    this._newUsers = true;
                    this._buffers.Add(e.Sender, buff = new MessageBuffer());
                }
            }

            lock (buff)
            {
                if (!e.Data.Any())
                {
                    buff.Reset();
                    return;
                }

                foreach (var b in e.Data)
                {
                    buff.AddByte(b);

                    var bytes = buff.CheckForMessages();
                    if (bytes != null)
                    {
                        this.Receive?.Invoke(this, new Message<T>(e.Sender, e.IsOwnMessage, bytes));
                    }
                }
            }
        }

        private class MessageBuffer
        {
            private readonly List<byte> _receivedBytes = new List<byte>();
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
                            this._state = ParseState.Body;
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
                this._length = 0;
                this._receivedBytes.Clear();
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