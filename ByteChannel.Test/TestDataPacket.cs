using System;

namespace ByteChannel.Test
{
    internal class TestDataPacket : IDataPacket<bool>
    {
        public TestDataPacket(int size)
        {
            this.Size = size;
        }
        public virtual void Send(byte[] data)
        {
            if (data.Length != this.Size) throw new NotSupportedException();

            this.Receive?.Invoke(this, new Message<bool>(true, true, data));
        }

        public event ReceiveCallback<Message<bool>> Receive;
        public int Size { get; }
        public bool IsBusy { get; } = false;
    }
}