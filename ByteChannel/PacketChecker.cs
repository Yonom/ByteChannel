using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ByteChannel
{
    internal class PacketChecker<TSender> : IChannel<OrderedMessage<TSender>>, IDisposable
    {
        private readonly AutoResetEvent _timeoutResetEvent = new AutoResetEvent(true);
        private readonly PacketPadder<TSender> _padder;
        private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> _sentQueue = new Queue<byte[]>();
        private readonly RegisteredWaitHandle _registration;
        private byte _pointer = 1;

        public PacketChecker(PacketPadder<TSender> padder)
        {
            this._padder = padder;
            this._padder.Receive += this._padder_Receive;
            this._registration = this.RegisterSendTimeout();
        }

        private RegisteredWaitHandle RegisterSendTimeout()
        {
            return ThreadPool.RegisterWaitForSingleObject(this._timeoutResetEvent, (state, timedOut) =>
            {
                if (!timedOut) return;
                if (this._padder.IsBusy) return;
                this.ResendMissed(null);
            }, null, 500, false);
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

        public event ReceiveCallback<OrderedMessage<TSender>> Receive;

        private void _padder_Receive(object sender, Message<TSender> e)
        {
            if (e.IsOwnMessage)
            {
                this._timeoutResetEvent.Set();

                this.ResendMissed(e.Data);
                this.FlushBuffer();
            }

            var location = (byte) (e.Data[0] - Config.CounterOffset);
            var data = ByteHelper.CropArray(e.Data, 1);
            this.Receive?.Invoke(this,
                new OrderedMessage<TSender>(
                    e.Sender, e.IsOwnMessage,
                    location, data));
        }

        private void ResendMissed(byte[] data)
        {
            lock (this._sentQueue)
            {
                var count = this._sentQueue.Count;
                while (count --> 0)
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
                while (this._sentQueue.Count < Config.MaxConcurrency && this._sendQueue.Any())
                {
                    this.SendInternal(ByteHelper.InsertByte(this.IncrementCounter(), this._sendQueue.Dequeue()));
                }
            }
        }

        private void SendInternal(byte[] data)
        {
            this._timeoutResetEvent.Set();

            this._sentQueue.Enqueue(data);
            this._padder.Send(data);
        }
        
        private byte IncrementCounter()
        {
            if (this._pointer == byte.MaxValue)
                this._pointer = Config.CounterOffset - 1;

            return ++this._pointer;
        }

        public void Dispose()
        {
            this._registration.Unregister(null);
            this._timeoutResetEvent.Dispose();
        }
    }
}