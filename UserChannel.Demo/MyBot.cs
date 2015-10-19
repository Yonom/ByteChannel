using System;
using System.IO;
using System.Threading;
using BotBits;
using BotBits.Events;
using BotCake;
using ByteChannel;
using ByteProtocol;

namespace UserChannel.Demo
{
    internal class MyBot : BotBase
    {
        private ByteChannel<Player> _byteChannel;
        
        [EventListener]
        public void On(JoinCompleteEvent e)
        {
            this._byteChannel = new ByteChannel<Player>(new CoinChannel());
            var bp = new ByteProtocol<Player>(this._byteChannel);
            var up = new UserProtocol<Player>(bp.RegisterProtocol("ByetUserStreams.Demo"), p => (uint)p.UserId);
            var stream = new ByteStream(up.GetUserChannel(this.Players.OwnPlayer, this.Players.OwnPlayer));
            {
                var writer = new StreamWriter(stream) {AutoFlush = true};
                writer.Write("Hello world!\n");
                var reader = new StreamReader(stream);
                ThreadPool.QueueUserWorkItem(o =>
                {
                    while (true)
                    {
                        Console.Write((char) reader.Read());
                    }
                });
            }
        }
    }
}