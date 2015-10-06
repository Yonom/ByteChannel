using System;
using System.Collections.Generic;
using System.Linq;

namespace ByteChannel
{
    internal class PacketBuffer<T> : IChannel<Message<T>>
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<T, MessageBuffer> _buffers = new Dictionary<T, MessageBuffer>();
        private readonly PacketChecker<T> _checker;

        public PacketBuffer(PacketChecker<T> checker)
        {
            this._checker = checker;
            this._checker.Receive += this._checker_Receive;
        }

        private void _checker_Receive(object sender, OrderedMessage<T> e)
        {
            MessageBuffer buff;
            lock (this._buffers)
            {
                if (!this._buffers.TryGetValue(e.Sender, out buff))
                {
                    this._buffers.Add(e.Sender, buff = new MessageBuffer());
                }
            }

            lock (buff)
            {
                buff.Set(e);

                foreach (var bytes in  buff.Read())
                {
                    this.Receive?.Invoke(this, new Message<T>(e.Sender, e.IsOwnMessage, bytes));
                }
            }
        }

        public void Send(byte[] data)
        {
            lock (this._lockObj)
            {
                if (data.Length <= this._checker.MaxSize) // makes sure we send new byte[0]s too
                {
                    this._checker.Send(data);
                    return;
                }

                foreach (var bytes in data.Slices(this._checker.MaxSize))
                    this._checker.Send(bytes);
            }
        }

        public event ReceiveCallback<Message<T>> Receive;

        private class MessageBuffer
        {
            private readonly byte[][] _buffer = new byte[Config.MaxQueueSize + 1][];
            private int _pointer = -1;

            public void Set(OrderedMessage<T> message)
            {
                this._buffer[message.Location] = message.Data;

                if (this._pointer == -1 && !message.Data.Any())
                    this._pointer = message.Location;
            }

            public IEnumerable<byte[]> Read()
            {
                if (this._pointer == -1) yield break;
                for (;; this._pointer++)
                {
                    if (this._pointer > Config.MaxQueueSize)
                        this._pointer = 0;

                    var data = this._buffer[this._pointer];
                    if (data == null)
                        yield break;

                    this._buffer[this._pointer] = null;
                    yield return data;
                }
            }
        }
    }
}