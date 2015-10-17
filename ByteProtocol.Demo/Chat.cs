using BotBits;

namespace ByteProtocol.Demo
{
    internal class Chat
    {
        public Chat(Player sender, string text)
        {
            this.Sender = sender;
            this.Text = text;
        }

        public string Text { get; }
        public Player Sender { get; }
    }
}