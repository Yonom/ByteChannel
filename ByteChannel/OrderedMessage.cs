using System;

namespace ByteChannel
{
    internal class OrderedMessage<T> : Message<T, ArraySegment<byte>>
    {
        public OrderedMessage(T sender, bool isOwnMessage, byte location, ArraySegment<byte> data)
            : base(sender, isOwnMessage, data)
        {
            this.Location = location;
        }

        public byte Location { get; }
    }
}