using System;
using System.Collections.Generic;
using System.Linq;

namespace ByteChannel
{
    internal class PacketBuffer<TSender> : IChannel<byte[], SegmentMessage<TSender>>, IDisposable
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<TSender, MessageBuffer> _buffers = new Dictionary<TSender, MessageBuffer>();
        private readonly PacketChecker<TSender> _checker;

        public PacketBuffer(PacketChecker<TSender> checker)
        {
            this._checker = checker;
            this._checker.Receive += this._checker_Receive;
        }

        private void _checker_Receive(object sender, OrderedMessage<TSender> e)
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

                foreach (var bytes in buff.Read())
                {
                    this.Receive?.Invoke(this, new SegmentMessage<TSender>(e.Sender, e.IsOwnMessage, bytes));
                }
            }
        }

        public void Send(byte[] data)
        {
            lock (this._lockObj)
            {
                if (data.Length <= this._checker.MaxSize) // makes sure we send byte[0]s too
                {
                    this._checker.Send(new ArraySegment<byte>(data));
                    return;
                }

                foreach (var bytes in data.Slices(this._checker.MaxSize))
                    this._checker.Send(bytes);
            }
        }

        public void RemoveSender(TSender sender)
        {
            lock (this._buffers) this._buffers.Remove(sender);
        }

        public event ChannelCallback<SegmentMessage<TSender>> Receive;

        private class MessageBuffer
        {
            private readonly ArraySegment<byte>?[] _buffer = new ArraySegment<byte>?[Config.MaxQueueSize];
            private int _pointer = -1;

            public void Set(OrderedMessage<TSender> message)
            {
                this._buffer[message.Location] = message.Data;

                if (this._pointer == -1 && message.Data.Count == 0)
                    this._pointer = message.Location;
            }

            public IEnumerable<ArraySegment<byte>> Read()
            {
                if (this._pointer == -1) yield break;
                for (;; this._pointer++)
                {
                    if (this._pointer == Config.MaxQueueSize)
                        this._pointer = 0;

                    var data = this._buffer[this._pointer];
                    if (data == null)
                        yield break;

                    this._buffer[this._pointer] = null;
                    yield return data.Value;
                }
            }
        }

        public void Dispose()
        {
            this._checker.Dispose();
            this._buffers.Clear();
        }
    }
}