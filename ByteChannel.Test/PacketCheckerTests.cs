using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ByteChannel.Test
{
    [TestClass]
    public class PacketCheckerTests
    {
        [TestMethod]
        public void UnstableTest()
        {
            var checker =
                new PacketChecker<bool>(
                    new PacketPadder<bool>(new UnstableTestDataPacket(2)),
                    ChannelOptions.Default);
            var data = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            byte[] received = new byte[0];

            checker.Receive += (sender, message) => 
                received = ByteHelper.Combine(received, message.Data.ToArray());
            foreach (var bytes in data.Slices(checker.MaxSize))
                checker.Send(bytes);

            CollectionAssert.AreEqual(data, received);
        }
    }

    public static class ArraySegmentExtensions
    {
        public static T[] ToArray<T>(this ArraySegment<T> arraySegment)
        {
            return arraySegment.Array.Skip(arraySegment.Offset).Take(arraySegment.Count).ToArray();
        }
    }
}
