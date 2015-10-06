namespace ByteChannel
{
    public delegate void ReceiveCallback<in T>(object sender, T e);
}