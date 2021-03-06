using System;
using System.Text;
using BotBits;
using BotBits.Events;
using BotBits.SendMessages;
using BotCake;
using ByteChannel;

namespace ByteProtocol.Demo
{
    internal class CoinChannel : BotBase, INetworkChannel<Player>
    {
        public void Send(byte[] data)
        {
            this.Actions.GetCoin(
                BitConverter.ToInt32(data, 0),
                BitConverter.ToInt32(data, 4),
                BitConverter.ToUInt32(data, 8),
                BitConverter.ToUInt32(data, 12));
        }

        public event ChannelCallback<Message<Player>> Receive;
        public int Size { get; } = 16;
        public bool IsBusy => CoinSendMessage.Of(this).Count > 0;

        [EventListener]
        private void On(CoinEvent e)
        {
            var bytes = new byte[this.Size];
            Buffer.BlockCopy(new[] {e.GoldCoins, e.BlueCoins}, 0, bytes, 0, 8);
            Buffer.BlockCopy(new[] {e.X, e.Y}, 0, bytes, 8, 8);
            this.Receive?.Invoke(this, new Message<Player>(e.Player, e.Player == this.Players.OwnPlayer, bytes));
        }
    }
}