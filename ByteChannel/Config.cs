namespace ByteChannel
{
    internal static class Config
    {
        internal const int DefaultWaitTimeout = 500;
        internal const byte MaxQueueSize = byte.MaxValue;
        internal const byte CounterOffset = byte.MaxValue - MaxQueueSize + 1;
    }
}