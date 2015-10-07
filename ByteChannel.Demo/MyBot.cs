using System;
using System.Text;
using System.Threading;
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


            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    channel.Send(Encoding.UTF8.GetBytes(Console.ReadLine()));
                }
            });

        }

        private void Channel_Receive(object sender, Message<Player> e)
        {
            Console.WriteLine(e.Sender.Username + " : " + Encoding.UTF8.GetString(e.Data));
        }
    }
}