using System.Collections.Generic;
using System.Linq;

namespace ByteProtocol
{
    internal class ProtocolMap
    {
        private readonly Dictionary<byte, string> _protocols = new Dictionary<byte, string>();

        public bool Set(byte protocolId, string protocolName)
        {
            lock (this._protocols)
            {
                var contains = this._protocols.ContainsKey(protocolId);
                this._protocols[protocolId] = protocolName;
                return !contains;
            }
        }

        public string Get(byte protocolId)
        {
            string name;
            lock (this._protocols) this._protocols.TryGetValue(protocolId, out name);
            return name;
        }

        public bool Remove(byte protocolId)
        {
            lock (this._protocols) return this._protocols.Remove(protocolId);
        }

        public string[] ToArray()
        {
            lock (this._protocols)
            {
                return this._protocols.Values.ToArray();
            }
        }
    }
}