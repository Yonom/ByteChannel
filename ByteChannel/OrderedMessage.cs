namespace ByteChannel
{
    internal class OrderedMessage<T> : Message<T>
    {
        public OrderedMessage(T sender, bool isOwnMessage, byte location, byte[] data)
            : base(sender, isOwnMessage, data)
        {
            this.Location = location;
        }

        public byte Location { get; }
    }
}