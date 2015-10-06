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
            var temp = new byte[size];
            Buffer.BlockCopy(data, 0, temp, size - data.Length, data.Length);
            return temp;
        }

        public static byte[] RemoveTrail(byte[] data)
        {
            var i = 0;
            while (i < data.Length && data[i] == 0) ++i;
            return CropArray(data, i);
        }

        public static byte[] CropArray(byte[] data, int toRemove)
        {
            var temp = new byte[data.Length - toRemove];
            Buffer.BlockCopy(data, toRemove, temp, 0, data.Length - toRemove);
            return temp;
        }

        public static byte[] InsertByte(byte newByte, byte[] data)
        {
            var temp = new byte[data.Length + 1];
            temp[0] = newByte;
            Buffer.BlockCopy(data, 0, temp, 1, data.Length);
            return temp;
        }

        // Copyright (c) 2008-2013 Hafthor Stefansson
        // Distributed under the MIT/X11 software license
        // Ref: http://www.opensource.org/licenses/mit-license.php.
        public static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                var l = a1.Length;
                for (var i = 0; i < l/8; i++, x1 += 8, x2 += 8)
                    if (*((long*) x1) != *((long*) x2)) return false;
                if ((l & 4) != 0)
                {
                    if (*((int*) x1) != *((int*) x2)) return false;
                    x1 += 4;
                    x2 += 4;
                }
                if ((l & 2) != 0)
                {
                    if (*((short*) x1) != *((short*) x2)) return false;
                    x1 += 2;
                    x2 += 2;
                }
                if ((l & 1) != 0) if (*x1 != *x2) return false;
                return true;
            }
        }

        public static T[] CopySlice<T>(this T[] source, int index, int length, bool padToLength = false)
        {
            int n = length;
            T[] slice = null;

            if (source.Length < index + length)
            {
                n = source.Length - index;
                if (padToLength)
                {
                    slice = new T[length];
                }
            }

            if (slice == null) slice = new T[n];
            Array.Copy(source, index, slice, 0, n);
            return slice;
        }

        public static IEnumerable<T[]> Slices<T>(this T[] source, int count, bool padToLength = false)
        {
            for (var i = 0; i < source.Length; i += count)
                yield return source.CopySlice(i, count, padToLength);
        }
    }
}