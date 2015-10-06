using System;

namespace ByteChannel
{
    public class Message<T> : EventArgs
    {
        public Message(T sender, bool isOwnMessage, byte[] data)
        {
            this.Sender = sender;
            this.IsOwnMessage = isOwnMessage;
            this.Data = data;
        }

        public bool IsOwnMessage { get; }
        public T Sender { get; }
        public byte[] Data { get; }
    }
}