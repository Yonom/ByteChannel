namespace ByteChannel
{
    internal static class Config
    {
        internal const byte MaxQueueSize = byte.MaxValue - 1;
        internal const byte CounterOffset = byte.MaxValue - MaxQueueSize;
    }
}