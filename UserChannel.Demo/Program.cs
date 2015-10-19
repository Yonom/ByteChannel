using BotBits;
using BotCake;

namespace UserChannel.Demo
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
