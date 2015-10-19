using System;
using System.Text;
using BotBits;
using ByteChannel;

namespace ByteProtocol.Demo
{
    internal class ChatChannel : IChannel<string, Chat>, IDisposable
    {
        private readonly IMessageChannel<Player> _channel;

        public ChatChannel(IMessageChannel<Player> channel)
        {
            this._channel = channel;
            this._channel.DiscoverSender += this._channel_DiscoverSender;
            this._channel.RemovedSender += this._channel_RemovedSender;
            this._channel.Receive += this.Channel_Receive;
        }

        public void Dispose()
        {
            this._channel.DiscoverSender -= this._channel_DiscoverSender;
            this._channel.RemovedSender -= this._channel_RemovedSender;
            this._channel.Receive -= this.Channel_Receive;
        }

        private void _channel_DiscoverSender(object sender, Player e)
        {
            Console.WriteLine(e.Username + " joined the room.");
        }

        private void _channel_RemovedSender(object sender, Player e)
        {
            Console.WriteLine(e.Username + " left the room.");
        }

        private void Channel_Receive(object sender, Message<Player> e)
        {
            this.Receive?.Invoke(this,
                new Chat(e.Sender, Encoding.UTF8.GetString(e.Data)));
        }

        public void Send(string data)
        {
            this._channel.Send(Encoding.UTF8.GetBytes(data));
        }

        public event ChannelCallback<Chat> Receive;
    }
}