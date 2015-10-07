namespace ByteChannel
{
    /// <summary>
    /// Encapsulates a method that handles received messages.
    /// </summary>
    /// <typeparam name="T">The type of the arguments.</typeparam>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    public delegate void ReceiveCallback<in T>(object sender, T e);
}