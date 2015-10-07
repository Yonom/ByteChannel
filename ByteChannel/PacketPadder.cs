namespace ByteChannel
{
    internal class PacketPadder<TSender> : IChannel<Message<TSender>>
    {
        private readonly IDataPacket<TSender> _packet;

        public PacketPadder(IDataPacket<TSender> packet)
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

        public event ReceiveCallback<Message<TSender>> Receive;

        private void _packet_Receive(object sender, Message<TSender> e)
        {
            this.Receive?.Invoke(this,
                new Message<TSender>(e.Sender, e.IsOwnMessage,
                    ByteHelper.RemoveTrail(e.Data)));
        }
    }
}