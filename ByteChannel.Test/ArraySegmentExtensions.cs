using System;
using System.Linq;

namespace ByteChannel.Test
{
    public static class ArraySegmentExtensions
    {
        public static T[] ToArray<T>(this ArraySegment<T> arraySegment)
        {
            return arraySegment.Array.Skip(arraySegment.Offset).Take(arraySegment.Count).ToArray();
        }
    }
}