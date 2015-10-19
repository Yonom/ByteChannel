using System;

namespace ByteChannel
{
    internal struct SegmentMessage<TSender>
    {
        public SegmentMessage(TSender sender, bool isOwnMessage, ArraySegment<byte> data)
        {
            this.Sender = sender;
            this.IsOwnMessage = isOwnMessage;
            this.Data = data;
        }
        public bool IsOwnMessage { get; }
        public TSender Sender { get; }
        public ArraySegment<byte> Data { get; }
    }
}