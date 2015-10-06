using System.Collections.Generic;
using System.IO;
using System.Linq;
using BotBits;
using BotCake;

namespace ByteChannel.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            CakeServices.Run(bot =>
            {
                ConnectionManager.Of(bot).GuestLogin().CreateJoinRoom("PW01");
                return new MyBot();
            });
        }
    }
}
