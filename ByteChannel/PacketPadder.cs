using System;

namespace ByteChannel
{
    internal class PacketPadder<TSender> : IChannel<byte[], SegmentMessage<TSender>>, IDisposable
    {
        private readonly INetworkChannel<TSender> _networkChannel;

        public PacketPadder(INetworkChannel<TSender> networkChannel)
        {
            this._networkChannel = networkChannel;
            this._networkChannel.Receive += this._networkChannel_Receive;
        }

        public void Dispose()
        {
            this._networkChannel.Receive -= this._networkChannel_Receive;
        }

        public int MaxSize => this._networkChannel.Size;
        public bool IsBusy => this._networkChannel.IsBusy;

        public void Send(byte[] data)
        {
            this._networkChannel.Send(ByteHelper.AddTrail(data, this._networkChannel.Size));
        }

        public event ChannelCallback<SegmentMessage<TSender>> Receive;

        private void _networkChannel_Receive(object sender, Message<TSender> e)
        {
            this.Receive?.Invoke(this,
                new SegmentMessage<TSender>(e.Sender, e.IsOwnMessage,
                    ByteHelper.RemoveTrail(e.Data)));
        }
    }
}