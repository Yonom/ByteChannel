using ByteChannel;

namespace ByteProtocol
{
    internal sealed class ProtocolChannel<TSender> : IByteChannel<TSender>
    {
        private readonly ByteProtocol<TSender> _protocol;
        public byte Id { get; }
        public string Name { get; }
        public bool Announced { get; set; }

        public ProtocolChannel(ByteProtocol<TSender> protocol, string name, byte id)
        {
            this.Name = name;
            this.Id = id;
            this._protocol = protocol;
        }

        public void Send(byte[] data)
        {
            this._protocol.Send(this, data);
        }

        public void Reset()
        {
            this._protocol.Reset(this);
        }

        public void RemoveSender(TSender sender)
        {
            this._protocol.RemoveSender(this, sender);
        }

        public event ChannelCallback<TSender> DiscoverSender;

        public void OnDiscoverSender(TSender e)
        {
            this.DiscoverSender?.Invoke(this, e);
        }

        public event ChannelCallback<TSender> RemovedSender;

        public void OnRemovedSender(TSender e)
        {
            this.RemovedSender?.Invoke(this, e);
        }

        public event ChannelCallback<Message<TSender>> Receive;

        public void OnReceive(Message<TSender> e)
        {
            this.Receive?.Invoke(this, e);
        }
    }
}