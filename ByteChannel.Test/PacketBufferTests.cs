using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ByteChannel.Test
{
    [TestClass]
    public class PacketBufferTests
    {
        [TestMethod]
        public void BufferUnstableTest()
        {
            var buffer =
                new PacketBuffer<bool>(
                    new PacketChecker<bool>(
                        new PacketPadder<bool>(new UnstableTestChannel(2)),
                        ByteChannelOptions.Default));
            var data = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            byte[] received = new byte[0];

            buffer.Receive += (sender, message) =>
                received = ByteHelper.Combine(received, message.Data.ToArray());
            buffer.Send(new byte[0]);
            buffer.Send(data);

            CollectionAssert.AreEqual(data, received);
        }

    }
}
