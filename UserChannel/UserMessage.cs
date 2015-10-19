namespace UserChannel
{
    public struct UserMessage<TSender>
    {
        public UserMessage(TSender sender, bool isOwnMessage, uint target, byte[] data)
        {
            this.Sender = sender;
            this.IsOwnMessage = isOwnMessage;
            this.Data = data;
            this.Target = target;
        }

        public bool IsOwnMessage { get; }
        public TSender Sender { get; }
        public byte[] Data { get; }
        public uint Target { get; }
    }
}