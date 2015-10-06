namespace ByteChannel
{
    internal class PacketPadder<T> : IChannel<Message<T>>
    {
        private readonly IDataPacket<T> _packet;

        public PacketPadder(IDataPacket<T> packet)
        {
            this._packet = packet;
            this._packet.Receive += this._packet_Receive;
        }

        public int MaxSize => this._packet.Size;

        public void Send(byte[] data)
        {
            this._packet.Send(ByteHelper.AddTrail(data, this._packet.Size));
        }

        public event ReceiveCallback<Message<T>> Receive;

        private void _packet_Receive(object sender, Message<T> e)
        {
            this.Receive?.Invoke(this,
                new Message<T>(e.Sender, e.IsOwnMessage,
                    ByteHelper.RemoveTrail(e.Data)));
        }
    }
}