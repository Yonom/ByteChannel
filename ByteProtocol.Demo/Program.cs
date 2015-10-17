using BotBits;
using BotCake;

namespace ByteProtocol.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CakeServices.Run(bot =>
            {
                ConnectionManager.Of(bot).GuestLogin().CreateJoinRoom("PW01");
                return new MyBot();
            });
        }
    }
}