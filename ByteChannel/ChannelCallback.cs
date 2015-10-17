namespace ByteChannel
{
    /// <summary>
    /// Encapsulates a method that handles a channel's output.
    /// </summary>
    /// <typeparam name="TOutput">The type of the argument.</typeparam>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    public delegate void ChannelCallback<in TOutput>(object sender, TOutput e);
}