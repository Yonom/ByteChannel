using System;
using System.Collections.Generic;
using System.Linq;

namespace ByteChannel
{
    // TODO add timeout
    internal class PacketChecker<T> : IChannel<OrderedMessage<T>>
    {
        private readonly PacketPadder<T> _padder;
        private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> _sentQueue = new Queue<byte[]>();
        private byte _pointer = 1;

        public PacketChecker(PacketPadder<T> padder)
        {
            this._padder = padder;
            this._padder.Receive += this._padder_Receive;
        }

        public int MaxSize => this._padder.MaxSize - 1;

        public void Send(byte[] data)
        {
            lock (this._sentQueue)
            {
                this._sendQueue.Enqueue(data);
                this.FlushBuffer();
            }
        }

        public event ReceiveCallback<OrderedMessage<T>> Receive;

        private void _padder_Receive(object sender, Message<T> e)
        {
            if (e.IsOwnMessage)
            {
                this.ResendMissed(e.Data);
                this.FlushBuffer();
            }

            var location = (byte) (e.Data[0] - Config.CounterOffset);
            var data = ByteHelper.CropArray(e.Data, 1);
            this.Receive?.Invoke(this,
                new OrderedMessage<T>(
                    e.Sender, e.IsOwnMessage,
                    location, data));
        }

        private void ResendMissed(byte[] data)
        {
            lock (this._sentQueue)
            {
                while (this._sentQueue.Any())
                {
                    var item = this._sentQueue.Dequeue();
                    if (ByteHelper.UnsafeCompare(item, data)) break;
                    this.SendInternal(item);
                }
            }
        }

        private void FlushBuffer()
        {
            lock (this._sentQueue)
            {
                while (this._sentQueue.Count < Config.MaxQueueSize && this._sendQueue.Any())
                {
                    this.SendInternal(ByteHelper.InsertByte(this.IncrementCounter(), this._sendQueue.Dequeue()));
                }
            }
        }

        private void SendInternal(byte[] data)
        {
            this._sentQueue.Enqueue(data);
            this._padder.Send(data);
        }
        
        private byte IncrementCounter()
        {
            if (this._pointer == byte.MaxValue)
                this._pointer = Config.CounterOffset - 1;

            return ++this._pointer;
        }
    }
}