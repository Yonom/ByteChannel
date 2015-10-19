using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ByteChannel;

namespace ByteProtocol.Demo
{
    class CompressionChannel<TSender> : IMessageChannel<TSender>, IDisposable
    {
        private readonly IMessageChannel<TSender> _innerChannel;

        public CompressionChannel(IMessageChannel<TSender> innerChannel)
        {
            this._innerChannel = innerChannel;
            this._innerChannel.Receive += this._innerChannel_Receive;
        }

        public void Dispose()
        {
            this._innerChannel.Receive -= this._innerChannel_Receive;
        }

        private void _innerChannel_Receive(object sender, Message<TSender> e)
        {
            using (var compressedStream = new MemoryStream(e.Data))
            using (var compressor = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                compressor.CopyTo(resultStream);
                this.Receive?.Invoke(this, new Message<TSender>(e.Sender, e.IsOwnMessage, resultStream.ToArray()));
            }
        }

        public void Send(byte[] data)
        { 
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                compressor.Write(data, 0, data.Length);
                compressor.Close();
                this._innerChannel.Send(compressStream.ToArray());
            }
        }

        public event ChannelCallback<Message<TSender>> Receive;

        public void Reset()
        {
            this._innerChannel.Reset();
        }

        public void RemoveSender(TSender sender)
        {
            this._innerChannel.RemoveSender(sender);
        }

        public event ChannelCallback<TSender> DiscoverSender
        {
            add { this._innerChannel.DiscoverSender += value; }
            remove { this._innerChannel.DiscoverSender -= value; }
        }
        public event ChannelCallback<TSender> RemovedSender
        {
            add { this._innerChannel.RemovedSender += value; }
            remove { this._innerChannel.RemovedSender -= value; }
        }
    }
}
