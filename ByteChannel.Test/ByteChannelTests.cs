using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ByteChannel.Test
{
    [TestClass]
    public class ByteChannelTests
    {
        [TestMethod]
        public void MessageTest()
        {
            var channel =new ByteChannel<bool>(new UnstableTestChannel(2));
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] received = new byte[0];

            channel.Receive += (sender, message) => received = message.Data;
            channel.Send(data);

            CollectionAssert.AreEqual(data, received);
        }

        [TestMethod]
        public void LotsOfMessagesTest()
        {
            var channel = new ByteChannel<bool>(new UnstableTestChannel(16));
            byte[] received = new byte[0];

            channel.Receive += (sender, message) => received = ByteHelper.Combine(received, message.Data);
            foreach (var line in Enumerable.Range(1, 1000))//File.ReadAllLines(@"C:\Users\Sepehr\Desktop\test.txt"))
                channel.Send(BitConverter.GetBytes(line));

            //CollectionAssert.AreEqual(data, received);
        }
    }
}
