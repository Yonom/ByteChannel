using System;
using System.Text;
using BotBits;
using BotBits.Events;
using BotCake;

namespace ByteChannel.Demo
{
    class MyBot : BotBase
    {
        [EventListener]
        public void On(JoinCompleteEvent e)
        {
            var channel = new ByteChannel<Player>(new CoinDataPacket());
            channel.Receive += this.Channel_Receive;
            channel.Send(Encoding.UTF8.GetBytes("Hello world!"));
        }

        private void Channel_Receive(object sender, Message<Player> e)
        {
            Console.WriteLine(Encoding.UTF8.GetString(e.Data));
        }
    }
}