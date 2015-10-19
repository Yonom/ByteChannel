using System;
using System.Collections.Concurrent;
using System.IO;

namespace UserChannel
{
    public class ByteStream : Stream
    {
        private readonly IUserChannel _channel;

        private readonly BlockingCollection<byte> _readStream =
            new BlockingCollection<byte>(new ConcurrentQueue<byte>());
        private readonly ConcurrentQueue<byte> _writeStream = new ConcurrentQueue<byte>();
        private readonly object _readLock = new object();
        private readonly object _writeLock = new object();

        public ByteStream(IUserChannel channel)
        {
            this._channel = channel;
            this._channel.Receive += this._channel_Receive;
        }

        public override void Close()
        {
            this._channel.Receive -= this._channel_Receive;
            lock (this._readStream) this._readStream.CompleteAdding();
            this.Flush();
            base.Close();
        }

        private void _channel_Receive(object sender, byte[] e)
        {
            lock (this._readStream)
            {
                foreach (byte b in e)
                    this._readStream.Add(b);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (this._readLock)
            {
                if (this._readStream.IsCompleted) return 0;
                buffer[offset] = this._readStream.Take();

                var i = 1;
                for (; i < count && this._readStream.Count > 0; i++)
                    buffer[offset + i] = this._readStream.Take();
                return i;
            }
        }

        public override int ReadByte()
        {
            lock (this._readLock)
            {
                return this._readStream.Take();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (this._writeLock)
            {
                for (var i = 0; i < count; i++)
                    this._writeStream.Enqueue(buffer[offset + i]);
            }
        }

        public override void WriteByte(byte value)
        {
            lock (this._writeLock)
            {
                this._writeStream.Enqueue(value);
            }
        }

        public override void Flush()
        {
            lock (this._writeStream)
            {
                if (this._writeStream.Count == 0) return;
                var bytes = new byte[this._writeStream.Count];
                var i = 0;
                while (i < bytes.Length)
                {
                    byte b;
                    this._writeStream.TryDequeue(out b); // must always be true
                    bytes[i++] = b;
                }
                this._channel.Send(bytes);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead { get; } = true;
        public override bool CanWrite { get; } = true;
        public override bool CanSeek { get; } = false;
        public override bool CanTimeout { get; } = false;

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
    }
}