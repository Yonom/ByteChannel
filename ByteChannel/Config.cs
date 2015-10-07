namespace ByteChannel
{
    internal static class Config
    {
        internal const byte MaxConcurrency = MaxQueueSize;
        internal const byte MaxQueueSize = byte.MaxValue;
        internal const byte CounterOffset = byte.MaxValue - MaxQueueSize + 1;
    }
}