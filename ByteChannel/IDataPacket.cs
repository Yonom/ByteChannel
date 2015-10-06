namespace ByteChannel
{
    public interface IDataPacket<T> : IChannel<Message<T>>
    {
        int Size { get; }
    }
}