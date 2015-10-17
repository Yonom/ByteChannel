using System;
using System.Text;

namespace ByteProtocol
{
    internal static class ByteHelper
    {
        public static byte[] SkipByte(byte[] data)
        {
            var temp = new byte[data.Length - 1];
            Buffer.BlockCopy(data, 1, temp, 0, data.Length - 1);
            return temp;
        }

        public static byte[] InsertByte(byte newByte, byte[] data)
        {
            var temp = new byte[data.Length + 1];
            temp[0] = newByte;
            Buffer.BlockCopy(data, 0, temp, 1, data.Length);
            return temp;
        }
    }
}