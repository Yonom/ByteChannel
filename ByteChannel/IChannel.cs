namespace ByteChannel
{
    public interface IChannel<out T>
    {
        void Send(byte[] data);
        event ReceiveCallback<T> Receive;
    }
}