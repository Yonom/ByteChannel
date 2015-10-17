using System;
using System.Collections.Generic;
using System.Linq;
using ByteChannel;

namespace ByteProtocol
{
    public class ByteProtocol<TSender> : IDisposable
    {
        private const string ProtocolName = "ByteProtocol1";
        private ByteProtocolChannel<TSender> _byteProtocol;
        private readonly IByteChannel<TSender> _channel;
        private readonly Dictionary<string, ProtocolChannel<TSender>> _protocols = new Dictionary<string, ProtocolChannel<TSender>>();

        public ByteProtocol(IByteChannel<TSender> channel)
        {
            this._channel = channel;
            this._channel.Receive += this._channel_Receive;
            this._channel.RemovedSender += this._channel_RemovedSender;
            
            this.RegisteProtocolInternal(ProtocolName, p =>
            {
                this._byteProtocol = new ByteProtocolChannel<TSender>(p);
                this._byteProtocol.ProtocolDiscovered += this._byteProtocol_ProtocolDiscovered;
            });
        }

        public void Dispose()
        {
            this._channel.Receive -= this._channel_Receive;
            this._channel.RemovedSender -= this._channel_RemovedSender;
        }

        public IByteChannel<TSender> RegisterProtocol(string protocolName)
        {
            IByteChannel<TSender> proto = null;
            this.RegisteProtocolInternal(protocolName, p => proto = p);
            return proto;
        }

        private void RegisteProtocolInternal(string protocolName, Action<IByteChannel<TSender>> callback)
        {
            if (string.IsNullOrEmpty(protocolName))
                throw new ArgumentNullException(nameof(protocolName));
            if (this._protocols.Count > byte.MaxValue)
                throw new InvalidOperationException("Only 256 protocols can be registered at once.");

            ProtocolChannel<TSender> proto;
            lock (this._protocols)
            {
                if (this._protocols.ContainsKey(protocolName))
                    throw new InvalidOperationException("The given protocol is already registered!");

                var protocolId = (byte)this._protocols.Count;
                proto = new ProtocolChannel<TSender>(this, protocolName, protocolId);
                this._protocols.Add(protocolName, proto);
            }

            callback(proto);
            this.Reset(proto);
        }

        //public bool UnregisterProtocol(string protocolName)
        //{
        //    lock (this._protocols)
        //    {
        //        Protocol<TSender> proto;
        //        if (this._protocols.TryGetValue(protocolName, out proto))
        //        {
        //            this._byteProtocol.SetProtocol(proto.)
        //        }
        //        return this._protocols.Remove(protocolName);
        //    }
        //}

        internal void Send(ProtocolChannel<TSender> protocolChannel, byte[] bytes)
        {
            if (!protocolChannel.Announced)
                lock (this._protocols)
                    if (!protocolChannel.Announced)
                        this.Reset(protocolChannel);

            this._channel.Send(ByteHelper.InsertByte(protocolChannel.Id, bytes));
        }

        internal void Reset(ProtocolChannel<TSender> protocolChannel)
        {
            protocolChannel.Announced = true;
            this._byteProtocol.SetProtocol(protocolChannel.Id, protocolChannel.Name);
        }

        internal void RemoveSender(ProtocolChannel<TSender> protocolChannel, TSender sender)
        {
            this._byteProtocol.RemoveSender(sender, protocolChannel.Id);
            protocolChannel.OnRemovedSender(sender);
        }

        private void _channel_Receive(object sender, Message<TSender> e)
        {
            if (!e.Data.Any()) return;

            var id = e.Data[0];
            var name = this._byteProtocol.GetProtocol(e.Sender, id);
            if (name == null)
            {
                if (id != 0) return;
                if (e.Data.Length != ProtocolName.Length + 2) return;
            }

            var data = ByteHelper.SkipByte(e.Data);
            if (name == null)
            {
                var m = ByteProtocolMessage.ParseMessage(data);
                if (m.Id != 0 || m.Name != ProtocolName) return;
                name = ProtocolName;
            }

            ProtocolChannel<TSender> proto;
            lock (this._protocols) this._protocols.TryGetValue(name, out proto);
            proto?.OnReceive(new Message<TSender>(e.Sender, e.IsOwnMessage, data));
        }

        private void _channel_RemovedSender(object sender, TSender e)
        {
            var protocols = this._byteProtocol.GetProtocols(e);
            if (protocols != null)
                lock (this._protocols)
                    foreach (var proto in this._protocols.Values.Where(p => protocols.Contains(p.Name)))
                        this.RemoveSender(proto, e);
            this._byteProtocol.RemoveSender(e);
        }
        
        private void _byteProtocol_ProtocolDiscovered(object sender, KeyValuePair<TSender, string> e)
        {
            ProtocolChannel<TSender> proto;
            lock (this._protocols) this._protocols.TryGetValue(e.Value, out proto);
            if (proto != null)
            {
                proto.Announced = false;
                proto.OnDiscoverSender(e.Key);
            }
        }
    }
}