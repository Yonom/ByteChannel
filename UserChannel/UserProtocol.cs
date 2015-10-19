using System;
using ByteChannel;

namespace UserChannel
{
    public class UserProtocol<TSender> : IDisposable
    {
        private readonly IMessageChannel<TSender> _channel;
        private readonly Func<TSender, uint> _identifyCallback;

        public UserProtocol(IMessageChannel<TSender> channel, Func<TSender, uint> identifyCallback)
        {
            this._channel = channel;
            this._identifyCallback = identifyCallback;
            this._channel.Receive += this._channel_Receive;
        }

        public IUserChannel GetUserChannel(TSender source, TSender target)
        {
            return new Channel(this, this._identifyCallback, source, target);
        }

        public void Dispose()
        {
            this._channel.Receive -= this._channel_Receive;
        }

        public void Send(TSender target, byte[] data)
        {
            var targetBytes = ByteHelper.EncodeVarint(this._identifyCallback(target));
            this._channel.Send(ByteHelper.Combine(targetBytes, data));
        }

        private void _channel_Receive(object sender, Message<TSender> e)
        {
            var i = 0;
            var target = 0u;
            for (; i < e.Data.Length; i++)
            {
                var b = e.Data[i];
                target |= ((uint)b & 0x7f) << (7 * i++);
                if ((b & 0x80) != 0x80) break;
            }

            this.Receive?.Invoke(this, 
                new UserMessage<TSender>(e.Sender, e.IsOwnMessage, target, ByteHelper.SkipBytes(e.Data, i)));
        }

        public event ChannelCallback<UserMessage<TSender>> Receive;

        class Channel : IUserChannel
        {
            private readonly UserProtocol<TSender> _channel;
            private readonly Func<TSender, uint> _identifyCallback;
            private readonly TSender _source;
            private readonly TSender _target;

            public Channel(UserProtocol<TSender> channel, Func<TSender, uint> identifyCallback, TSender source, TSender target)
            {
                this._channel = channel;
                this._identifyCallback = identifyCallback;
                this._source = source;
                this._target = target;
                this._channel.Receive += this._channel_Receive;
            }

            public void Dispose()
            {
                this._channel.Receive -= this._channel_Receive;
            }

            public void Send(byte[] data)
            {
                this._channel.Send(this._target, data);
            }

            private void _channel_Receive(object sender, UserMessage<TSender> e)
            {
                if (this._identifyCallback(this._target) != this._identifyCallback(e.Sender) || 
                    this._identifyCallback(this._source) != e.Target) return;
                this.Receive?.Invoke(this, e.Data);
            }

            public event ChannelCallback<byte[]> Receive;
        }
    }
}
