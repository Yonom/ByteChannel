using System.Text;

namespace ByteProtocol
{
    internal struct ByteProtocolMessage
    {
        public byte Id { get; set; }
        public string Name { get; set; }

        public static ByteProtocolMessage ParseMessage(byte[] data)
        {
            return new ByteProtocolMessage
            {
                Id = data[0],
                Name = Encoding.UTF8.GetString(ByteHelper.SkipByte(data))
            };
        }
    }
}