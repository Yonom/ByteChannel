using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ByteChannel.Test
{
    [TestClass]
    public class TestDataPacketTests
    {
        [TestMethod]
        public void InvalidDataTest()
        {
            var packet = new TestChannel(16);

            try
            {
                packet.Send(new byte[0]);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        public void ReceiveTest()
        {
            var packet = new TestChannel(16);
            var received = false;

            packet.Receive += (sender, message) => received = true;
            packet.Send(new byte[packet.Size]);

            Assert.IsTrue(received);
        }
    }
}