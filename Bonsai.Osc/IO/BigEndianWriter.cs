using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Bonsai.Osc.IO
{
    class BigEndianWriter : BinaryWriter
    {
        public BigEndianWriter(Stream output)
            : this(output, false)
        {
        }

        public BigEndianWriter(Stream output, bool leaveOpen)
            : base(output, Encoding.BigEndianUnicode, leaveOpen)
        {
        }

        public override void Write(char ch)
        {
            base.Write(IPAddress.HostToNetworkOrder((short)ch));
        }

        public override void Write(char[] chars)
        {
            var bytes = Encoding.BigEndianUnicode.GetBytes(chars);
            base.Write(bytes);
        }

        public override void Write(char[] chars, int index, int count)
        {
            var bytes = Encoding.BigEndianUnicode.GetBytes(chars, index, count);
            base.Write(bytes);
        }

        public override void Write(decimal value)
        {
            throw new NotSupportedException();
        }

        public override void Write(double value)
        {
            var bits = BitConverter.DoubleToInt64Bits(value);
            base.Write(IPAddress.HostToNetworkOrder(bits));
        }

        public override void Write(float value)
        {
            base.Write(IPAddress.HostToNetworkOrder(value.GetHashCode()));
        }

        public override void Write(int value)
        {
            base.Write(IPAddress.HostToNetworkOrder(value));
        }

        public override void Write(long value)
        {
            base.Write(IPAddress.HostToNetworkOrder(value));
        }

        public override void Write(short value)
        {
            base.Write(IPAddress.HostToNetworkOrder(value));
        }

        public override void Write(string value)
        {
            var bytes = Encoding.BigEndianUnicode.GetBytes(value);
            Write7BitEncodedInt(bytes.Length);
            base.Write(bytes);
        }

        public override void Write(uint value)
        {
            base.Write(IPAddress.HostToNetworkOrder((int)value));
        }

        public override void Write(ulong value)
        {
            base.Write(IPAddress.HostToNetworkOrder((long)value));
        }

        public override void Write(ushort value)
        {
            base.Write(IPAddress.HostToNetworkOrder((short)value));
        }
    }
}
