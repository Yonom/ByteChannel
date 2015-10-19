using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BotBits;
using BotBits.Events;
using BotCake;
using ByteChannel;

namespace ByteProtocol.Demo
{
    internal class MyBot : BotBase
    {
        private readonly ByteChannel<Player> _byteChannel = new ByteChannel<Player>(new CoinChannel());

        [EventListener]
        public void On(JoinCompleteEvent e)
        {
            var bp = new ByteProtocol<Player>(this._byteChannel);
            var cc = new ChatChannel(new CompressionChannel<Player>(bp.RegisterProtocol("EE Chat")));

            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    cc.Send(Console.ReadLine());
                }
            });

            cc.Receive += this.Cc_Receive;
        }

        [EventListener]
        public void On(LeaveEvent e)
        {
            this._byteChannel.RemoveSender(e.Player);
        }

        private void Cc_Receive(object sender, Chat e)
        {
            Console.WriteLine(e.Sender.Username + ": " + e.Text);
        }
    }
}