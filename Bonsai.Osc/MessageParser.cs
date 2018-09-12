using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    static class MessageParser
    {
        const int PadLength = 4;
        const string ReadBlobMethodName = "ReadBlob";
        const string ReadCharMethodName = "ReadChar";
        const string ReadStringMethodName = "ReadString";
        const string ReadTimeTagMethodName = "ReadTimeTag";
        static readonly MethodInfo ReadBytes = typeof(BinaryReader).GetMethod("ReadBytes");
        static readonly MethodInfo ReadInt32 = typeof(BinaryReader).GetMethod("ReadInt32");
        static readonly MethodInfo ReadInt64 = typeof(BinaryReader).GetMethod("ReadInt64");
        static readonly MethodInfo ReadUInt64 = typeof(BinaryReader).GetMethod("ReadUInt64");
        static readonly MethodInfo ReadFloat = typeof(BinaryReader).GetMethod("ReadSingle");
        static readonly MethodInfo ReadDouble = typeof(BinaryReader).GetMethod("ReadDouble");

        internal static DateTimeOffset ReadTimeTag(BinaryReader reader)
        {
            var timeTag = reader.ReadUInt64();
            return TimeTag.ToTimestamp(timeTag);
        }

        internal static byte[] ReadBlob(BinaryReader reader)
        {
            var blobSize = reader.ReadInt32();
            return ReadBlob(reader, blobSize);
        }

        internal static byte[] ReadBlob(BinaryReader reader, int blobSize)
        {
            var bytes = reader.ReadBytes(blobSize);
            var zeroBytes = blobSize % PadLength;
            if (zeroBytes > 0)
            {
                reader.ReadBytes(zeroBytes);
            }

            return bytes;
        }

        internal static char ReadChar(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(PadLength);
            return Encoding.ASCII.GetChars(bytes)[0];
        }

        internal static string ReadString(BinaryReader reader)
        {
            int index = 0;
            var done = false;
            var bytes = new byte[PadLength];
            while (!done)
            {
                var bytesRead = reader.Read(bytes, index, PadLength);
                if (bytesRead < PadLength)
                {
                    throw new InvalidOperationException("OSC string was not in a correct format.");
                }

                if ((bytesRead = Array.IndexOf<byte>(bytes, 0, index, PadLength)) >= 0)
                {
                    index = bytesRead;
                    done = true;
                }
                else
                {
                    index += PadLength;
                    if (index >= bytes.Length)
                    {
                        Array.Resize(ref bytes, bytes.Length * 2);
                    }
                }
            }

            return Encoding.ASCII.GetString(bytes, 0, index);
        }

        internal static Expression Address(Expression reader)
        {
            return Expression.Call(typeof(MessageParser), ReadStringMethodName, null, reader);
        }

        internal static Expression Content(string typeTag, Expression reader)
        {
            if (string.IsNullOrEmpty(typeTag))
            {
                throw new ArgumentException("A valid type tag must be specified.", "typeTag");
            }

            var chars = typeTag.ToArray();
            var tupleCreate = typeof(Tuple).GetMethods()
                .Where(method => method.Name == "Create" && method.GetParameters().Length == chars.Length)
                .FirstOrDefault();
            if (tupleCreate == null)
            {
                throw new ArgumentException("OSC messages with more than eight arguments are not supported.", "typeTag");
            }

            var arguments = Array.ConvertAll(chars, tag =>
            {
                switch (tag)
                {
                    case TypeTag.Char:
                        return Expression.Call(typeof(MessageParser), ReadCharMethodName, null, reader);
                    case TypeTag.TimeTag:
                        return Expression.Call(typeof(MessageParser), ReadTimeTagMethodName, null, reader);
                    case TypeTag.Int64: return Expression.Call(reader, ReadInt64);
                    case TypeTag.Int32: return Expression.Call(reader, ReadInt32);
                    case TypeTag.Float: return Expression.Call(reader, ReadFloat);
                    case TypeTag.Double: return Expression.Call(reader, ReadDouble);
                    case TypeTag.Alternate:
                    case TypeTag.String: return Address(reader);
                    case TypeTag.Blob:
                        var blobSize = Expression.Call(reader, ReadInt32);
                        return Expression.Call(typeof(MessageParser), ReadBlobMethodName, null, reader, blobSize);
                    default: throw new ArgumentException(string.Format("The type tag '{0}' is not supported.", tag), "typeTag");
                }
            });

            if (arguments.Length == 1) return arguments[0];
            else
            {
                var argumentTypes = arguments.Select(m => m.Type).ToArray();
                tupleCreate = tupleCreate.MakeGenericMethod(argumentTypes);
                return Expression.Call(tupleCreate, arguments);
            }
        }
    }
}
