using System.Collections.Generic;
using System.Text;
using ByteChannel;

namespace ByteProtocol
{
    internal class ByteProtocolChannel<TSender>
    {
        private readonly IByteChannel<TSender> _channel;
        private readonly Dictionary<TSender, ProtocolMap> _users = new Dictionary<TSender, ProtocolMap>();

        public ByteProtocolChannel(IByteChannel<TSender> channel)
        {
            this._channel = channel;
            this._channel.Receive += this._channel_Receive;
            this._channel.RemovedSender += this._channel_RemovedSender;
        }

        private void _channel_RemovedSender(object sender, TSender e)
        {
            lock (this._users)
            {
                this._users.Remove(e);
            }
        }

        public string[] GetProtocols(TSender sender)
        {
            ProtocolMap map;
            lock (this._users) if (!this._users.TryGetValue(sender, out map)) return null;
            return map.ToArray();
        }

        public string GetProtocol(TSender sender, byte protocolId)
        {
            ProtocolMap map;
            lock (this._users) if (!this._users.TryGetValue(sender, out map)) return null;
            return map.Get(protocolId);
        }

        public void SetProtocol(byte protocolId, string protocolName)
        {
            this._channel.Send(ByteHelper.InsertByte(protocolId, Encoding.UTF8.GetBytes(protocolName)));
        }

        public bool RemoveSender(TSender sender)
        {
            lock (this._users) return this._users.Remove(sender);
        }

        public bool RemoveSender(TSender sender, byte protocolId)
        {
            ProtocolMap map;
            lock (this._users) if (!this._users.TryGetValue(sender, out map)) return false;
            return map.Remove(protocolId);
        }

        private void _channel_Receive(object sender, Message<TSender> e)
        {
            ProtocolMap map;
            lock (this._users)
            {
                if (!this._users.TryGetValue(e.Sender, out map))
                {
                    this._users.Add(e.Sender, map = new ProtocolMap());
                }
            }
            var m = ByteProtocolMessage.ParseMessage(e.Data);
            if (map.Set(m.Id, m.Name))
                this.ProtocolDiscovered?.Invoke(this, new KeyValuePair<TSender, string>(e.Sender, m.Name));
        }

        public event ChannelCallback<KeyValuePair<TSender, string>> ProtocolDiscovered;
    }
}