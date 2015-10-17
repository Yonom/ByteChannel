using System;

namespace ByteChannel
{
    internal class PacketPadder<TSender> : IChannel<byte[], SegmentMessage<TSender>>, IDisposable
    {
        private readonly INetworkChannel<TSender> _packet;

        public PacketPadder(INetworkChannel<TSender> packet)
        {
            this._packet = packet;
            this._packet.Receive += this._packet_Receive;
        }

        public int MaxSize => this._packet.Size;
        public bool IsBusy => this._packet.IsBusy;

        public void Send(byte[] data)
        {
            this._packet.Send(ByteHelper.AddTrail(data, this._packet.Size));
        }

        public event ChannelCallback<SegmentMessage<TSender>> Receive;

        private void _packet_Receive(object sender, Message<TSender> e)
        {
            this.Receive?.Invoke(this,
                new SegmentMessage<TSender>(e.Sender, e.IsOwnMessage,
                    ByteHelper.RemoveTrail(e.Data)));
        }

        public void Dispose()
        {
            this._packet.Receive -= this._packet_Receive;
        }
    }
}