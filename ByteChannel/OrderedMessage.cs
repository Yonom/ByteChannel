﻿using System;

namespace ByteChannel
{
    internal struct OrderedMessage<TSender>
    {
        public OrderedMessage(TSender sender, bool isOwnMessage, byte location, ArraySegment<byte> data)
        {
            this.Sender = sender;
            this.IsOwnMessage = isOwnMessage;
            this.Data = data;
            this.Location = location;
        }

        public bool IsOwnMessage { get; }
        public TSender Sender { get; }
        public ArraySegment<byte> Data { get; }
        public byte Location { get; }
    }
}