﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteChannel.Test
{
    internal class UnstableTestChannel : TestChannel
    {
        public UnstableTestChannel(int size) : base(size)
        {
        }

        private int toggle;
        public override void Send(byte[] data)
        {
            if (toggle++ % 3 == 0) return; // intentionally miss

            base.Send(data);
        }
    }
}
