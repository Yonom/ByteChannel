using System;

namespace ByteChannel.Test
{
    internal class TestChannel : INetworkChannel<bool>
    {
        public TestChannel(int size)
        {
            this.Size = size;
        }
        public virtual void Send(byte[] data)
        {
            if (data.Length != this.Size) throw new NotSupportedException();

            this.Receive?.Invoke(this, new Message<bool>(true, true, data));
        }

        public event ChannelCallback<Message<bool>> Receive;
        public int Size { get; }
        public bool IsBusy { get; } = false;
    }
}