using System;
using System.Collections.Generic;
using System.Linq;
using ByteChannel;

namespace ByteProtocol
{
    /// <summary>
    /// Represents a IByteChannel factory that is implemented using ByteProtocol.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    public class ByteProtocol<TSender> : IDisposable
    {
        private const string ProtocolName = "ByteProto V1";
        private ByteProtocolChannel<TSender> _byteProtocol;
        private readonly IMessageChannel<TSender> _channel;
        private readonly Dictionary<string, ProtocolChannel<TSender>> _protocols = new Dictionary<string, ProtocolChannel<TSender>>();
        private readonly HashSet<byte> _usedIds = new HashSet<byte>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteProtocol{TSender}"/> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public ByteProtocol(IMessageChannel<TSender> channel)
        {
            this._channel = channel;
            this._channel.Receive += this._channel_Receive;
            this._channel.RemovedSender += this._channel_RemovedSender;

            this.RegisterProtocolInternal(ProtocolName, p =>
            {
                this._byteProtocol = new ByteProtocolChannel<TSender>(p);
                this._byteProtocol.ProtocolDiscovered += this._byteProtocol_ProtocolDiscovered;
                this._byteProtocol.ProtocolDeleted += this._byteProtocol_ProtocolDeleted;
            });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this._channel.Receive -= this._channel_Receive;
            this._channel.RemovedSender -= this._channel_RemovedSender;
        }

        /// <summary>
        /// Registers a protocol using the given unique protocol name.
        /// </summary>
        /// <param name="protocolName">Name of the protocol.</param>
        /// <returns></returns>
        public IMessageChannel<TSender> RegisterProtocol(string protocolName)
        {
            IMessageChannel<TSender> proto = null;
            this.RegisterProtocolInternal(protocolName, p => proto = p);
            return proto;
        }

        private void RegisterProtocolInternal(string protocolName, Action<IMessageChannel<TSender>> callback)
        {
            if (string.IsNullOrEmpty(protocolName))
                throw new ArgumentNullException(nameof(protocolName));

            ProtocolChannel<TSender> proto;
            lock (this._protocols)
            {
                if (this._protocols.ContainsKey(protocolName))
                    throw new InvalidOperationException("The given protocol is already registered!");

                var protocolId = this.GetFreeId();
                proto = new ProtocolChannel<TSender>(this, protocolName, protocolId);
                this._protocols.Add(protocolName, proto);
            }

            callback(proto);
            this.Reset(proto);
        }

        /// <summary>
        /// Unregisters a protocol.
        /// </summary>
        /// <param name="protocolName">Name of the protocol.</param>
        /// <returns></returns>
        public bool UnregisterProtocol(string protocolName)
        {
            lock (this._protocols)
            {
                ProtocolChannel<TSender> proto;
                if (this._protocols.TryGetValue(protocolName, out proto))
                {
                    this._byteProtocol.SetProtocol(proto.Id, String.Empty);
                    this._usedIds.Remove(proto.Id);
                }
                return this._protocols.Remove(protocolName);
            }
        }

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

        private byte GetFreeId()
        {
            if (this._usedIds.Count > byte.MaxValue)
                throw new InvalidOperationException("Only 256 protocols can be registered at once.");

            for (int i = byte.MinValue; i <= byte.MaxValue; i++)
            {
                var b = (byte)i;
                if (this._usedIds.Contains(b)) continue;

                this._usedIds.Add(b);
                return b;
            }

            // This code should never run
            throw new InvalidOperationException("Unable to find an available protocol id.");
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

        private void _byteProtocol_ProtocolDiscovered(object sender, KeyValuePair<Message<TSender>, string> e)
        {
            ProtocolChannel<TSender> proto;
            lock (this._protocols) this._protocols.TryGetValue(e.Value, out proto);
            if (proto != null)
            {
                if (!e.Key.IsOwnMessage) proto.Announced = false;
                proto.OnDiscoverSender(e.Key.Sender);
            }
        }

        private void _byteProtocol_ProtocolDeleted(object sender, KeyValuePair<Message<TSender>, string> e)
        {
            ProtocolChannel<TSender> proto;
            lock (this._protocols) this._protocols.TryGetValue(e.Value, out proto);
            if (proto != null)
            {
                this.RemoveSender(proto, e.Key.Sender);
            }
        }
    }
}