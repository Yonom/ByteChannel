using System;
using System.Collections.Generic;
using System.Linq;

namespace ByteChannel
{
    internal static class ByteHelper
    {
        public static byte[] EncodeVarint(int value)
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

        public static void SplitBytes(byte[] bytes, int position, out byte[] bytes1, out byte[] bytes2)
        {
            bytes1 = new byte[position];
            bytes2 = new byte[bytes.Length - position];
            Buffer.BlockCopy(bytes, 0, bytes1, 0, position);
            Buffer.BlockCopy(bytes, position, bytes1, 0, bytes2.Length);
        }

        public static byte[] AddTrail(byte[] data, int size)
        {
            if (size == data.Length) return data;

            var temp = new byte[size];
            Buffer.BlockCopy(data, 0, temp, size - data.Length, data.Length);
            return temp;
        }

        public static ArraySegment<byte> RemoveTrail(byte[] data)
        {
            var i = 0;
            while (i < data.Length && data[i] == 0) ++i;
            return CropArray(data, i);
        }

        public static ArraySegment<byte> CropArray(byte[] data, int toRemove)
        {
            return new ArraySegment<byte>(data, toRemove, data.Length - toRemove);
        }

        public static ArraySegment<byte> CropArray(ArraySegment<byte> data, int toRemove)
        {
            return new ArraySegment<byte>(data.Array, data.Offset + toRemove, data.Count - toRemove);
        }

        public static byte[] InsertByte(byte newByte, ArraySegment<byte> data)
        {
            var temp = new byte[data.Count + 1];
            temp[0] = newByte;
            Buffer.BlockCopy(data.Array, data.Offset, temp, 1, data.Count);
            return temp;
        }

        public static bool Compare(byte[] a1, ArraySegment<byte> a2)
        {
            if (a1.Length != a2.Count) return false;
            for (var i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2.Array[i + a2.Offset])
                {
                    return false;
                }
            }
            return true;
        }

        public static ArraySegment<T> CreateSlice<T>(this T[] source, int index, int length)
        {
            var n = length;

            if (source.Length < index + length)
                n = source.Length - index;
           
            return new ArraySegment<T>(source, index, n);
        }

        public static IEnumerable<ArraySegment<T>> Slices<T>(this T[] source, int count)
        {
            for (var i = 0; i < source.Length; i += count)
                yield return source.CreateSlice(i, count);
        }
    }
}