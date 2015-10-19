using System;
using System.Linq;

namespace UserChannel
{
    internal static class ByteHelper
    {
        public static byte[] SkipBytes(byte[] data, int count)
        {
            var temp = new byte[data.Length - count];
            Buffer.BlockCopy(data, count, temp, 0, temp.Length);
            return temp;
        }

        public static byte[] EncodeVarint(uint value)
        {
            var buffer = new byte[4];
            var pos = 0;
            do
            {
                var byteVal = value & 0x7f;
                value >>= 7;

                if (value != 0)
                {
                    byteVal |= 0x80;
                }

                buffer[pos++] = (byte)byteVal;

            } while (value != 0);

            var result = new byte[pos];
            Buffer.BlockCopy(buffer, 0, result, 0, pos);

            return result;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            var rv = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}