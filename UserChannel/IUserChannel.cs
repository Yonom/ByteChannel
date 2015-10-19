using System;
using ByteChannel;

namespace UserChannel
{
    public interface IUserChannel : IChannel<byte[], byte[]>, IDisposable
    {
    }
}