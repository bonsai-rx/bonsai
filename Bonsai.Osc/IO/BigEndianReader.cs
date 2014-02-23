using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Bonsai.Osc.IO
{
    class BigEndianReader : BinaryReader
    {
        public BigEndianReader(Stream input)
            : this(input, false)
        {
        }

        public BigEndianReader(Stream input, bool leaveOpen)
            : base(input, Encoding.BigEndianUnicode, leaveOpen)
        {
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct SingleAndInt32Union
        {
            [FieldOffset(0)]
            public int Int32;
            [FieldOffset(0)]
            public float Single;
        }

        public override char ReadChar()
        {
            return (char)IPAddress.NetworkToHostOrder(base.ReadInt16());
        }

        public override char[] ReadChars(int count)
        {
            var byteCount = count * sizeof(char);
            var bytes = base.ReadBytes(byteCount);
            return Encoding.BigEndianUnicode.GetChars(bytes);
        }

        public override decimal ReadDecimal()
        {
            throw new NotSupportedException();
        }

        public override double ReadDouble()
        {
            var bits = IPAddress.NetworkToHostOrder(base.ReadInt64());
            return BitConverter.Int64BitsToDouble(bits);
        }

        public override short ReadInt16()
        {
            return IPAddress.NetworkToHostOrder(base.ReadInt16());
        }

        public override int ReadInt32()
        {
            return IPAddress.NetworkToHostOrder(base.ReadInt32());
        }

        public override long ReadInt64()
        {
            return IPAddress.NetworkToHostOrder(base.ReadInt64());
        }

        public override float ReadSingle()
        {
            var union = default(SingleAndInt32Union);
            union.Int32 = IPAddress.NetworkToHostOrder(base.ReadInt32());
            return union.Single;
        }

        public override string ReadString()
        {
            var byteCount = Read7BitEncodedInt();
            var bytes = ReadBytes(byteCount);
            return Encoding.BigEndianUnicode.GetString(bytes);
        }

        public override ushort ReadUInt16()
        {
            return (ushort)IPAddress.NetworkToHostOrder(base.ReadInt16());
        }

        public override uint ReadUInt32()
        {
            return (uint)IPAddress.NetworkToHostOrder(base.ReadInt32());
        }

        public override ulong ReadUInt64()
        {
            return (ulong)IPAddress.NetworkToHostOrder(base.ReadInt64());
        }
    }
}
