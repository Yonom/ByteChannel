using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ByteChannel.Test
{
    [TestClass]
    public class PacketPadderTests
    {
        [TestMethod]
        public void PadTest()
        {
            var padder = new PacketPadder<bool>(new TestDataPacket(16));
            var data = new byte[] {1, 2, 3, 4, 5};
            byte[] received = null;

            padder.Receive += (sender, message) => received = message.Data;
            padder.Send(data);

            CollectionAssert.AreEqual(data, received);
        }
    }
}